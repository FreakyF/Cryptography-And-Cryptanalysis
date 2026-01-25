using System.Numerics;

namespace Task01.Domain.Services;

public static class StatisticalTestService
{
    private static int CountOnes(byte[] data)
    {
        var count = 0;
        foreach (var b in data)
        {
            count += BitOperations.PopCount(b);
        }

        return count;
    }

    public static double CalculateChiSquare(byte[] keystream)
    {
        var nBits = keystream.Length * 8;
        var observedOnes = CountOnes(keystream);
        var observedZeros = nBits - observedOnes;
        var expected = nBits / 2.0;

        var chiSq = System.Math.Pow(observedZeros - expected, 2) / expected +
                    System.Math.Pow(observedOnes - expected, 2) / expected;
        return chiSq;
    }

    public static void RunTests(byte[] keystream)
    {
        var nBits = keystream.Length * 8;
        var ones = CountOnes(keystream);
        var freq = (double)ones / nBits;

        Console.WriteLine($"Length: {nBits}");
        Console.WriteLine($"Frequency (Ones): {freq:P2} (Exp: 50%)");

        var runs = 1;
        var lastBit = keystream[0] & 1;

        for (var i = 0; i < keystream.Length; i++)
        {
            var currentByte = keystream[i];
            for (var b = 0; b < 8; b++)
            {
                if (i == 0 && b == 0)
                {
                    continue;
                }

                var currentBit = (currentByte >> b) & 1;
                if (currentBit != lastBit)
                {
                    runs++;
                    lastBit = currentBit;
                }
            }
        }

        var expectedRuns = 2.0 * ones * (nBits - ones) / nBits + 1;
        Console.WriteLine($"Runs: {runs} (Exp: {expectedRuns:F0})");

        var matches = 0;
        lastBit = keystream[0] & 1;

        for (var i = 0; i < keystream.Length; i++)
        {
            var currentByte = keystream[i];
            for (var b = 0; b < 8; b++)
            {
                if (i == 0 && b == 0)
                {
                    continue;
                }

                var currentBit = (currentByte >> b) & 1;
                if (currentBit == lastBit)
                {
                    matches++;
                }

                lastBit = currentBit;
            }
        }

        var mismatches = nBits - 1 - matches;
        var autocorr = (double)(matches - mismatches) / (nBits - 1);
        Console.WriteLine($"Autocorrelation (Lag 1): {autocorr:F4} (Exp: < 0.1)");

        Console.WriteLine($"Chi-Square Statistic: {CalculateChiSquare(keystream):F4} (Critical Value α=0.05: 3.841)");
    }
}