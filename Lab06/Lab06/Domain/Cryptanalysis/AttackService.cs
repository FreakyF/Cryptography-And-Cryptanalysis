using Lab06.Domain.Generators;
using Lab06.Infrastructure.Utils;

namespace Lab06.Domain.Cryptanalysis;

public class AttackService
{
    private const int LenX = 3;
    private const int LenY = 4;
    private const int LenZ = 5;

    private readonly int[] _tapsX = [0, 1];
    private readonly int[] _tapsY = [0, 3];
    private readonly int[] _tapsZ = [0, 2];

    public AttackResult CorrelationAttack(int[] keystream)
    {
        Console.WriteLine("--- Starting Correlation Attack ---");

        var bestX = FindBestCorrelation(keystream, LenX, _tapsX, "X");
        var bestZ = FindBestCorrelation(keystream, LenZ, _tapsZ, "Z");

        var bestY = RecoverY(keystream, bestX, bestZ);

        return new AttackResult(bestX, bestY, bestZ);
    }

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

    private bool CheckKey(int[] keystream, int[] sx, int[] sy, int[] sz)
    {
        var lx = new Lfsr(LenX, _tapsX, sx);
        var ly = new Lfsr(LenY, _tapsY, sy);
        var lz = new Lfsr(LenZ, _tapsZ, sz);
        var gen = new CombinationGenerator(lx, ly, lz);

        return keystream.All(bit => gen.NextBit() == bit);
    }
}