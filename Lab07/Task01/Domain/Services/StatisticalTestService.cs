using System.Numerics;

namespace Task01.Domain.Services;

/// <summary>
///     Provides a suite of statistical tests to evaluate the randomness properties of cryptographic keystreams.
/// </summary>
public static class StatisticalTestService
{
    /// <summary>
    ///     Counts the total number of set bits (1s) in a byte array.
    /// </summary>
    /// <param name="data">The input data.</param>
    /// <returns>The total count of ones.</returns>
    private static int CountOnes(byte[] data)
    {
        var count = 0;
        foreach (var b in data)
        {
            count += BitOperations.PopCount(b);
        }

        return count;
    }

    /// <summary>
    ///     Calculates the Chi-Square statistic for the distribution of ones and zeros.
    /// </summary>
    /// <param name="keystream">The keystream data to analyze.</param>
    /// <returns>The calculated Chi-Square value.</returns>
    /// <remarks>
    ///     The test compares the observed frequencies of 0s and 1s against the expected frequency (50% each) for a random source.
    ///     A value exceeding the critical threshold (e.g., 3.841 for p=0.05, 1 degree of freedom) indicates deviation from randomness.
    /// </remarks>
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

    /// <summary>
    ///     Runs a battery of statistical tests on the provided keystream and prints the results to the console.
    /// </summary>
    /// <param name="keystream">The keystream data to test.</param>
    /// <remarks>
    ///     The following tests are performed:
    ///     <list type="bullet">
    ///         <item><description><b>Frequency Test (Monobit):</b> Checks if the proportion of ones is close to 0.5.</description></item>
    ///         <item><description><b>Runs Test:</b> Checks if the number of consecutive runs of identical bits matches expectations.</description></item>
    ///         <item><description><b>Autocorrelation Test (Lag 1):</b> Checks for correlations between adjacent bits.</description></item>
    ///         <item><description><b>Chi-Square Test:</b> Goodness-of-fit test for uniform distribution.</description></item>
    ///     </list>
    /// </remarks>
    public static void RunTests(byte[] keystream)
    {
        var nBits = keystream.Length * 8;
        var ones = CountOnes(keystream);
        var freq = (double)ones / nBits;

        Console.WriteLine($"Length: {nBits}");
        Console.WriteLine($"Frequency (Ones): {freq:P2} (Exp: 50%)");

        // Runs Test
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

        // Autocorrelation Test (Lag 1)
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
