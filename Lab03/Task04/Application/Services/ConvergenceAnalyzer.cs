using Task04.Domain.Abstractions;
using Task04.Domain.Models;
using Task04.Domain.Services;
using Task04.Infrastructure.Abstractions;

namespace Task04.Application.Services;

public sealed class ConvergenceAnalyzer(
    IAlgorithmRunner runner,
    IQualityEvaluator evaluator,
    ITextNormalizer normalizer,
    BigramWeights weights,
    ConvergenceContext ctx)
{
    public async Task<ConvergenceSeries> RunAsync(
        string algoName,
        IReadOnlyList<int> iterationCheckpoints,
        int repeats,
        CancellationToken ct = default)
    {
        var points = new List<ConvergencePoint>(iterationCheckpoints.Count);

        foreach (var iters in iterationCheckpoints)
        {
            var objVals = new double[repeats];
            var accVals = new double[repeats];

            for (int k = 0; k < repeats; k++)
            {
                var res = await runner.RunAsync(ctx.CipherPath, ctx.BigramsPath, ctx.WorkDir, iters, ct)
                    .ConfigureAwait(false);

                var decNorm = normalizer.Normalize(res.DecryptedText);

                objVals[k] = Score(decNorm);
                accVals[k] = evaluator.Evaluate(decNorm, ctx.ReferenceNormalized, res.RecoveredKey, null).textAcc;
            }

            points.Add(new ConvergencePoint(
                iters,
                Mean(objVals), Std(objVals),
                Mean(accVals), Std(accVals)));
        }

        return new ConvergenceSeries(algoName, points);
    }

    private double Score(string normalizedText) => new BigramScorer().Score(normalizedText, weights);

    private static double Mean(ReadOnlySpan<double> v)
    {
        if (v.Length == 0) return 0;
        double s = 0;
        foreach (var t in v) s += t;
        return s / v.Length;
    }

    private static double Std(ReadOnlySpan<double> v)
    {
        if (v.Length == 0) return 0;
        double m = Mean(v), s2 = 0;
        foreach (var t in v)
        {
            var d = t - m;
            s2 += d * d;
        }

        return Math.Sqrt(s2 / v.Length);
    }
}