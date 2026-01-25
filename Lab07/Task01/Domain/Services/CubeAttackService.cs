using System.Diagnostics;
using Task01.Domain.Core;
using Task01.Domain.Math;

namespace Task01.Domain.Services;

public record Cube(List<int> Indices);

public class CubeAttackService(ITriviumCipher cipher)
{
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

    public List<(Cube Cube, int KeyIndex)> FindLinearCubes(int rounds)
    {
        var found = new List<(Cube, int)>();
        var random = new Random(42);

        for (var size = 1; size <= 6; size++)
        {
            var swSize = Stopwatch.StartNew();
            var sizeCount = 0;
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

    private bool TryIdentifyLinearity(Cube cube, int rounds, out int keyIndex)
    {
        keyIndex = -1;
        var random = new Random();
        var candidates = Enumerable.Range(0, 80).ToList();

        for (var test = 0; test < 5; test++)
        {
            var testKey = new bool[80];
            for (var k = 0; k < 80; k++)
            {
                testKey[k] = random.Next(2) == 1;
            }

            var val = ComputeSuperpoly(cube, testKey, new bool[80], rounds);

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

    public static bool[] RecoverKey(List<(Cube Cube, int KeyIndex)> linearCubes, ITriviumCipher oracle, int rounds)
    {
        var swOnline = Stopwatch.StartNew();
        var results = new bool[linearCubes.Count];

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

        var matrix = new List<bool[]>();
        foreach (var item in linearCubes)
        {
            var row = new bool[80];
            row[item.KeyIndex] = true;
            matrix.Add(row);
        }

        var recoveredBits = Gf2Solver.SolveLinearSystem(matrix, results, 80);
        swOnline.Stop();
        Console.WriteLine($"Online phase: recovery took {swOnline.Elapsed.TotalMicroseconds:F2} μs");

        return recoveredBits;
    }
}