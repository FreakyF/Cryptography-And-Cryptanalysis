namespace Task01.Domain.Services;

public static class StatisticalTestService
{
    public static void RunTests(bool[] keystream)
    {
        var n = keystream.Length;
        var ones = keystream.Count(b => b);
        var freq = (double)ones / n;

        Console.WriteLine($"Length: {n}");
        Console.WriteLine($"Frequency (Ones): {freq:P2} (Exp: 50%)");

        var runs = 1;
        for (var i = 1; i < n; i++)
        {
            if (keystream[i] != keystream[i - 1]) runs++;
        }
        var expectedRuns = (2.0 * ones * (n - ones)) / n + 1;
        Console.WriteLine($"Runs: {runs} (Exp: {expectedRuns:F0})");

        var matches = 0;
        for (var i = 0; i < n - 1; i++)
        {
            if (keystream[i] == keystream[i + 1]) matches++;
        }
        var mismatches = (n - 1) - matches;
        var autocorr = (double)(matches - mismatches) / (n - 1);
        Console.WriteLine($"Autocorrelation (Lag 1): {autocorr:F4} (Exp: < 0.1)");
    }
}