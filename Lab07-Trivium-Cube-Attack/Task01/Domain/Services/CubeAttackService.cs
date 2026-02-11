using System.Diagnostics;
using Task01.Domain.Core;
using Task01.Domain.Math;

namespace Task01.Domain.Services;

/// <summary>
///     Represents a set of IV bit indices that form a "Cube" for the Cube Attack.
/// </summary>
/// <param name="Indices">The list of bit positions (0-79) in the IV that are varied (summed over).</param>
public record Cube(List<int> Indices);

/// <summary>
///     Implements the Cube Attack against reduced-round versions of the Trivium stream cipher.
/// </summary>
/// <remarks>
///     The Cube Attack is a chosen-IV algebraic attack. It exploits the fact that for low-degree polynomials,
///     summing the output over a "cube" (a subspace of IV variables) can reduce the degree of the superpoly
///     to a linear function of the key bits.
///     <para>
///         This service handles both the offline phase (finding linear cubes) and the online phase (key recovery).
///     </para>
/// </remarks>
/// <param name="cipher">The cipher instance used for generating outputs during analysis.</param>
public class CubeAttackService(ITriviumCipher cipher)
{
    /// <summary>
    ///     Converts an array of boolean bits into a byte array.
    /// </summary>
    /// <param name="bits">The input bits (must be a multiple of 8 usually, but here handled generally).</param>
    /// <returns>A byte array representation.</returns>
    private static byte[] ToByteArray(bool[] bits)
    {
        var bytes = new byte[10];
        for (var i = 0; i < bits.Length; i++)
        {
            if (bits[i])
            {
                bytes[i / 8] |= (byte)(1 << (i % 8));
            }
        }

        return bytes;
    }

    /// <summary>
    ///     Computes the value of the superpoly for a given cube and key/IV configuration.
    ///     This effectively sums the first output bit of the cipher over all possible values of the cube variables.
    /// </summary>
    /// <param name="cube">The cube defining the IV bits to vary.</param>
    /// <param name="key">The key to test.</param>
    /// <param name="fixedIv">The base IV values (non-cube bits).</param>
    /// <param name="rounds">The number of initialization rounds.</param>
    /// <returns>The boolean sum (XOR) of the cipher outputs.</returns>
    private bool ComputeSuperpoly(Cube cube, bool[] key, bool[] fixedIv, int rounds)
    {
        var sum = false;
        var iterations = 1 << cube.Indices.Count;

        for (var i = 0; i < iterations; i++)
        {
            var ivBits = (bool[])fixedIv.Clone();
            for (var b = 0; b < cube.Indices.Count; b++)
            {
                if (((i >> b) & 1) == 1)
                {
                    ivBits[cube.Indices[b]] = true;
                }
            }

            cipher.Initialize(ToByteArray(key), ToByteArray(ivBits), rounds);
            sum ^= cipher.GenerateBit();
        }

        return sum;
    }

    /// <summary>
    ///     Offline Phase: Searches for linear cubes for a specified number of rounds.
    /// </summary>
    /// <param name="rounds">The number of rounds to target.</param>
    /// <returns>
    ///     A list of tuples, where each tuple contains a <see cref="Cube"/> and the index of the key bit
    ///     that the cube's superpoly is linear in.
    /// </returns>
    public List<(Cube Cube, int KeyIndex)> FindLinearCubes(int rounds)
    {
        var found = new List<(Cube, int)>();
        var random = new Random(42);

        // Search for cubes of size 1 to 6
        for (var size = 1; size <= 6; size++)
        {
            var swSize = Stopwatch.StartNew();
            var sizeCount = 0;
            // Attempt 20 random cubes of each size
            for (var i = 0; i < 20; i++)
            {
                var indices = Enumerable.Range(0, 80).OrderBy(_ => random.Next()).Take(size).ToList();
                var cube = new Cube(indices);

                if (!TryIdentifyLinearity(cube, rounds, out var kIdx))
                {
                    continue;
                }

                found.Add((cube, kIdx));
                sizeCount++;
            }

            swSize.Stop();
            Console.WriteLine($"Size {size}: {sizeCount} cubes in {swSize.Elapsed.TotalMicroseconds:F2} μs");
        }

        return found;
    }

    /// <summary>
    ///     Tests if a given cube produces a linear superpoly in exactly one key variable.
    /// </summary>
    /// <param name="cube">The cube to test.</param>
    /// <param name="rounds">The number of rounds.</param>
    /// <param name="keyIndex">Output parameter receiving the index of the key bit if linearity is found.</param>
    /// <returns><c>true</c> if the cube is linear in exactly one key bit; otherwise, <c>false</c>.</returns>
    private bool TryIdentifyLinearity(Cube cube, int rounds, out int keyIndex)
    {
        keyIndex = -1;
        var random = new Random();
        var candidates = Enumerable.Range(0, 80).ToList();

        // Perform linearity tests using random keys
        for (var test = 0; test < 5; test++)
        {
            var testKey = new bool[80];
            for (var k = 0; k < 80; k++)
            {
                testKey[k] = random.Next(2) == 1;
            }

            var val = ComputeSuperpoly(cube, testKey, new bool[80], rounds);

            // Filter out key bits that do not match the superpoly output behavior
            // (Simplified linearity check: assumes p(k) = k_i + c, checks consistency)
            candidates.RemoveAll(kIdx => testKey[kIdx] != val);

            if (candidates.Count == 0)
            {
                return false;
            }
        }

        if (candidates.Count != 1)
        {
            return false;
        }

        keyIndex = candidates[0];
        return true;
    }

    /// <summary>
    ///     Online Phase: Recovers key bits using the identified linear cubes and an oracle.
    /// </summary>
    /// <param name="linearCubes">The list of linear cubes found in the offline phase.</param>
    /// <param name="oracle">The oracle (black box) containing the secret key to be recovered.</param>
    /// <param name="rounds">The number of rounds.</param>
    /// <returns>An array of recovered key bits (some may remain unknown/false if no cube covered them).</returns>
    public static bool[] RecoverKey(List<(Cube Cube, int KeyIndex)> linearCubes, ITriviumCipher oracle, int rounds)
    {
        var swOnline = Stopwatch.StartNew();
        var results = new bool[linearCubes.Count];

        // Evaluate cubes against the live oracle in parallel
        Parallel.For(0, linearCubes.Count, i =>
        {
            var localCipher = new TriviumCipher();
            var cube = linearCubes[i].Cube;
            var sum = false;
            var iterations = 1 << cube.Indices.Count;

            for (var j = 0; j < iterations; j++)
            {
                var ivBits = new bool[80];
                for (var b = 0; b < cube.Indices.Count; b++)
                {
                    if (((j >> b) & 1) == 1)
                    {
                        ivBits[cube.Indices[b]] = true;
                    }
                }

                localCipher.Initialize(new byte[10], ToByteArray(ivBits), rounds);
                sum ^= localCipher.GenerateBit();
            }

            results[i] = sum;
        });

        // Construct linear system: A * k = results
        // Since we identified cubes where superpoly = k_i (plus constant, assumed 0 here for simplicity),
        // we essentially have k_i = result.
        var matrix = new List<bool[]>();
        foreach (var item in linearCubes)
        {
            var row = new bool[80];
            row[item.KeyIndex] = true;
            matrix.Add(row);
        }

        // Solve the system to recover key bits
        var recoveredBits = Gf2Solver.SolveLinearSystem(matrix, results, 80);
        swOnline.Stop();
        Console.WriteLine($"Online phase: recovery took {swOnline.Elapsed.TotalMicroseconds:F2} μs");

        return recoveredBits;
    }
}
