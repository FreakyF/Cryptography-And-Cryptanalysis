using Task04.Domain.Abstractions;
using Task04.Domain.Models;

namespace Task04.Application.Services;

public sealed class PerformanceAnalyzer(
    IQualityEvaluator evaluator,
    string reference,
    double thresholdPercent,
    int repeats)
{
    private const int SearchAvgTrials = 5;
    private const int BaselineTrials = 5;

    public delegate Task<AlgorithmResult> AlgoInvoker(int iterations, CancellationToken ct);

    public async Task<PerformanceMetrics> MeasureAsync(string name, AlgoInvoker invoke, CancellationToken ct = default)
    {
        const int maxItersCap = 500_000_000;
        var iters = 1;
        int itersLower = 0, itersUpper = 0;

        while (iters <= maxItersCap)
        {
            var accMean = await MeanAccuracyAsync(invoke, iters, SearchAvgTrials, ct);
            if (accMean >= thresholdPercent)
            {
                itersUpper = iters;
                break;
            }

            itersLower = iters;
            iters <<= 1;
        }

        if (itersUpper == 0)
            return new PerformanceMetrics(name, null, null, null);

        int lo = itersLower + 1, hi = itersUpper, best = itersUpper;
        while (lo <= hi)
        {
            var mid = lo + ((hi - lo) >> 1);
            var accMean = await MeanAccuracyAsync(invoke, mid, SearchAvgTrials, ct);
            if (accMean >= thresholdPercent)
            {
                best = mid;
                hi = mid - 1;
            }
            else
            {
                lo = mid + 1;
            }
        }

        var baselineMs = await MeanTimeMsAsync(invoke, 1, BaselineTrials, ct);

        var times = new double[repeats];
        var accs = new double[repeats];

        for (var k = 0; k < repeats; k++)
        {
            var r = await invoke(best, ct);
            accs[k] = evaluator.Evaluate(r.DecryptedText, reference, r.RecoveredKey, null).textAcc;
            times[k] = Math.Max(0.0, r.ElapsedMilliseconds - baselineMs);
        }

        return new PerformanceMetrics(
            name,
            best,
            Mean(times),
            Mean(accs)
        );
    }

    private async Task<double> MeanAccuracyAsync(AlgoInvoker invoke, int iterations, int trials, CancellationToken ct)
    {
        double s = 0;
        for (var i = 0; i < trials; i++)
        {
            var r = await invoke(iterations, ct);
            s += evaluator.Evaluate(r.DecryptedText, reference, r.RecoveredKey, null).textAcc;
        }

        return s / trials;
    }

    private static async Task<double> MeanTimeMsAsync(AlgoInvoker invoke, int iterations, int trials, CancellationToken ct)
    {
        double s = 0;
        for (var i = 0; i < trials; i++)
        {
            var r = await invoke(iterations, ct);
            s += r.ElapsedMilliseconds;
        }

        return s / trials;
    }

    private static double Mean(ReadOnlySpan<double> v)
    {
        double s = 0;
        foreach (var t in v)
            s += t;

        return v.Length == 0 ? 0 : s / v.Length;
    }
}