namespace Task01.Domain.Services;

using Core;
using Math;

public record Cube(List<int> Indices);

public class CubeAttackService(ITriviumCipher cipher)
{
    private bool ComputeSuperpoly(Cube cube, bool[] key, bool[] fixedIv, int rounds)
    {
        var sum = false;
        var iterations = 1 << cube.Indices.Count;

        for (var i = 0; i < iterations; i++)
        {
            var iv = (bool[])fixedIv.Clone();
            for (var b = 0; b < cube.Indices.Count; b++)
            {
                if (((i >> b) & 1) == 1)
                    iv[cube.Indices[b]] = true;
            }

            cipher.Initialize(key, iv, rounds);
            sum ^= cipher.GenerateBit();
        }

        return sum;
    }

    public List<(Cube Cube, int KeyIndex)> FindLinearCubes(int rounds)
    {
        var found = new List<(Cube, int)>();
        var random = new Random(123);

        for (var size = 1; size <= 2; size++)
        {
            for (var i = 0; i < 100; i++)
            {
                var indices = Enumerable.Range(0, 80).OrderBy(_ => random.Next()).Take(size).ToList();
                var cube = new Cube(indices);

                if (TryIdentifyLinearity(cube, rounds, out var kIdx))
                {
                    found.Add((cube, kIdx));
                }
            }
        }

        return found;
    }

    private bool TryIdentifyLinearity(Cube cube, int rounds, out int keyIndex)
    {
        keyIndex = -1;
        var random = new Random();
        var candidates = Enumerable.Range(0, 80).ToList();

        for (var test = 0; test < 10; test++)
        {
            var testKey = new bool[80];
            for (var k = 0; k < 80; k++) testKey[k] = random.Next(2) == 1;

            var val = ComputeSuperpoly(cube, testKey, new bool[80], rounds);

            candidates.RemoveAll(kIdx => testKey[kIdx] != val);
            if (candidates.Count == 0) return false;
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
        var matrix = new List<bool[]>();
        var results = new bool[linearCubes.Count];
        var currentRow = 0;

        foreach (var item in linearCubes)
        {
            var sum = false;
            var iterations = 1 << item.Cube.Indices.Count;
            for (var i = 0; i < iterations; i++)
            {
                var iv = new bool[80];
                for (var b = 0; b < item.Cube.Indices.Count; b++)
                {
                    if (((i >> b) & 1) == 1)
                        iv[item.Cube.Indices[b]] = true;
                }
                
                oracle.Initialize(new bool[80], iv, rounds); 
                sum ^= oracle.GenerateBit();
            }

            results[currentRow] = sum;

            var row = new bool[80];
            row[item.KeyIndex] = true; 
            matrix.Add(row);

            currentRow++;
        }

        var recoveredBits = Gf2Solver.SolveLinearSystem(matrix, results, 80);
        return recoveredBits;
    }
}