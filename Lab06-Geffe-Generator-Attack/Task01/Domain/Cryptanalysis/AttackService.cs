using Lab06.Domain.Generators;
using Lab06.Infrastructure.Utils;

namespace Lab06.Domain.Cryptanalysis;

/// <summary>
/// Provides services for performing cryptanalytic attacks on the combination generator.
/// </summary>
public class AttackService
{
    /// <summary>
    /// The length (degree) of the first LFSR (X).
    /// </summary>
    private const int LenX = 3;

    /// <summary>
    /// The length (degree) of the second LFSR (Y).
    /// </summary>
    private const int LenY = 4;

    /// <summary>
    /// The length (degree) of the third LFSR (Z).
    /// </summary>
    private const int LenZ = 5;

    /// <summary>
    /// The feedback taps for the first LFSR (X).
    /// </summary>
    private readonly int[] _tapsX = [0, 1];

    /// <summary>
    /// The feedback taps for the second LFSR (Y).
    /// </summary>
    private readonly int[] _tapsY = [0, 3];

    /// <summary>
    /// The feedback taps for the third LFSR (Z).
    /// </summary>
    private readonly int[] _tapsZ = [0, 2];

    /// <summary>
    /// Performs a correlation attack to recover the initial states of the LFSRs.
    /// </summary>
    /// <param name="keystream">The captured keystream bits.</param>
    /// <returns>An <see cref="AttackResult"/> containing the recovered states.</returns>
    public AttackResult CorrelationAttack(int[] keystream)
    {
        Console.WriteLine("--- Starting Correlation Attack ---");

        var bestX = FindBestCorrelation(keystream, LenX, _tapsX, "X");
        var bestZ = FindBestCorrelation(keystream, LenZ, _tapsZ, "Z");

        var bestY = RecoverY(keystream, bestX, bestZ);

        return new AttackResult(bestX, bestY, bestZ);
    }

    /// <summary>
    /// Finds the initial state that maximizes the Pearson correlation between the generated sequence and the keystream.
    /// </summary>
    /// <param name="keystream">The captured keystream bits.</param>
    /// <param name="degree">The degree of the LFSR being analyzed.</param>
    /// <param name="taps">The feedback taps for the LFSR.</param>
    /// <param name="label">A label for logging purposes (e.g., "X" or "Z").</param>
    /// <returns>The initial state that produces the highest correlation.</returns>
    private static int[] FindBestCorrelation(int[] keystream, int degree, int[] taps, string label)
    {
        var maxRho = -2.0;
        var bestState = Array.Empty<int>();
        var limit = 1 << degree;

        Console.WriteLine($"Analyzing Register {label}...");

        for (var i = 1; i < limit; i++)
        {
            var candidateState = BitUtils.IntToBinaryArray(i, degree);
            var generatedSequence = BitUtils.GenerateLfsrSequence(keystream.Length, candidateState, taps);

            var rho = Statistics.PearsonCorrelation(keystream, generatedSequence);

            if (!(rho > maxRho))
            {
                continue;
            }

            maxRho = rho;
            bestState = candidateState;
        }

        Console.WriteLine($"Selected {label}: {string.Join("", bestState)} with Rho={maxRho:F4}");
        return bestState;
    }

    /// <summary>
    /// Recovers the state of the second LFSR (Y) assuming X and Z are known.
    /// </summary>
    /// <param name="keystream">The captured keystream bits.</param>
    /// <param name="stateX">The recovered state of X.</param>
    /// <param name="stateZ">The recovered state of Z.</param>
    /// <returns>The recovered state of Y.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the state of Y cannot be recovered.</exception>
    private int[] RecoverY(int[] keystream, int[] stateX, int[] stateZ)
    {
        Console.WriteLine("Recovering Register Y (Exhaustive search)...");
        const int limit = 1 << LenY;

        for (var i = 1; i < limit; i++)
        {
            var stateY = BitUtils.IntToBinaryArray(i, LenY);

            if (!CheckKey(keystream, stateX, stateY, stateZ))
            {
                continue;
            }

            Console.WriteLine($"Found Y: {string.Join("", stateY)}");
            return stateY;
        }

        throw new InvalidOperationException(
            "Could not recover register Y. The correlation attack may have failed for X or Z.");
    }

    /// <summary>
    /// Performs a brute-force attack to find the initial states by trying all possible combinations.
    /// </summary>
    /// <param name="keystream">The captured keystream bits.</param>
    /// <returns>An <see cref="AttackResult"/> containing the recovered states, or empty states if not found.</returns>
    public AttackResult BruteForceAttack(int[] keystream)
    {
        Console.WriteLine("--- Starting Brute Force Attack ---");
        const int limX = 1 << LenX;
        const int limY = 1 << LenY;
        const int limZ = 1 << LenZ;

        for (var ix = 1; ix < limX; ix++)
        {
            for (var iy = 1; iy < limY; iy++)
            {
                for (var iz = 1; iz < limZ; iz++)
                {
                    var sx = BitUtils.IntToBinaryArray(ix, LenX);
                    var sy = BitUtils.IntToBinaryArray(iy, LenY);
                    var sz = BitUtils.IntToBinaryArray(iz, LenZ);

                    if (CheckKey(keystream, sx, sy, sz))
                    {
                        return new AttackResult(sx, sy, sz);
                    }
                }
            }
        }

        return new AttackResult();
    }

    /// <summary>
    /// Checks if a candidate key (state set) generates the observed keystream.
    /// </summary>
    /// <param name="keystream">The observed keystream bits.</param>
    /// <param name="sx">The candidate state for X.</param>
    /// <param name="sy">The candidate state for Y.</param>
    /// <param name="sz">The candidate state for Z.</param>
    /// <returns><c>true</c> if the generated stream matches the keystream; otherwise, <c>false</c>.</returns>
    private bool CheckKey(int[] keystream, int[] sx, int[] sy, int[] sz)
    {
        var lx = new Lfsr(LenX, _tapsX, sx);
        var ly = new Lfsr(LenY, _tapsY, sy);
        var lz = new Lfsr(LenZ, _tapsZ, sz);
        var gen = new CombinationGenerator(lx, ly, lz);

        return keystream.All(bit => gen.NextBit() == bit);
    }
}
