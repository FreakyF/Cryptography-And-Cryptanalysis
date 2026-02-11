using Task04.Application.Models;
using Task04.Domain.Abstractions;
using Task04.Domain.Models;
using Task04.Domain.Services;
using Task04.Infrastructure.Abstractions;
using Task04.Infrastructure.Services;

namespace Task04.Application.Services;

public sealed class QualityComparisonOrchestrator(
    IAlgorithmRunner algoMh,
    IAlgorithmRunner algoSa,
    IQualityEvaluator evaluator,
    ITextNormalizer normalizer)
{
    public async Task RunAsync(QualityComparisonArgs args, CancellationToken ct = default)
    {
        var work = TempFiles.EnsureDir(args.WorkDir, ".task04");

        var refPlainTask = File.ReadAllTextAsync(args.PlainRefPath, ct);

        string? trueKey = null;
        if (args.TrueKeyPath is not null && File.Exists(args.TrueKeyPath))
        {
            trueKey = await File.ReadAllTextAsync(args.TrueKeyPath, ct).ConfigureAwait(false);
        }

        var refPlain = await refPlainTask.ConfigureAwait(false);

        var mh = await algoMh.RunAsync(args.CipherPath, args.BigramsPath, work, null, ct).ConfigureAwait(false);
        var sa = await algoSa.RunAsync(args.CipherPath, args.BigramsPath, work, null, ct).ConfigureAwait(false);

        var (mhTextAcc, mhKeyAcc) = evaluator.Evaluate(mh.DecryptedText, refPlain, mh.RecoveredKey, trueKey);
        var (saTextAcc, saKeyAcc) = evaluator.Evaluate(sa.DecryptedText, refPlain, sa.RecoveredKey, trueKey);

        var qualityRows = new List<QualityMetrics>
        {
            new("MH (Task02)", mhTextAcc, mhKeyAcc),
            new("SA (Task03)", saTextAcc, saKeyAcc)
        };

        ConsoleReport.PrintQualityTable(qualityRows);

        var perf = new PerformanceAnalyzer(evaluator, refPlain, 85.0, 10);

        var mhPerf = await perf.MeasureAsync("MH (Task02)", MhInvoker, ct).ConfigureAwait(false);

        var saPerf = await perf.MeasureAsync("SA (Task03)", SaInvoker, ct).ConfigureAwait(false);

        ConsoleReport.PrintPerformanceTable([mhPerf, saPerf]);
        
        var bigramsText = await File.ReadAllTextAsync(args.BigramsPath, ct).ConfigureAwait(false);
        var weights = new BigramScorer().LoadWeights(bigramsText);
        var refNorm = normalizer.Normalize(refPlain);
        var ctx = new ConvergenceContext(
            args.CipherPath,
            args.BigramsPath,
            work,
            refNorm);

        var checkpoints = new[]
        {
            1, 2, 4, 8, 16, 32, 64, 128, 256,
            512, 1_000, 2_000, 4_000, 8_000, 16_000,
            32_000, 64_000, 125_000, 250_000, 500_000
        };
        const int repeats = 10;

        var convMh = new ConvergenceAnalyzer(
            algoMh, evaluator, normalizer, weights, ctx);

        var convSa = new ConvergenceAnalyzer(
            algoSa, evaluator, normalizer, weights, ctx);
        var seriesMh = await convMh.RunAsync("MH (Task02)", checkpoints, repeats, ct).ConfigureAwait(false);

        var seriesSa = await convSa.RunAsync("SA (Task03)", checkpoints, repeats, ct).ConfigureAwait(false);

        ConsoleReport.PrintConvergenceHeader();
        
        AsciiLineChartsRenderer.RenderMetric(
            "Convergence – objective",
            seriesMh,
            seriesSa,
            p => p.MeanObjective);

        AsciiLineChartsRenderer.RenderMetric(
            "Convergence – text accuracy (%)",
            seriesMh,
            seriesSa,
            p => p.MeanTextAcc);
        return;

        Task<AlgorithmResult> SaInvoker(int iters, CancellationToken token) =>
            algoSa.RunAsync(args.CipherPath, args.BigramsPath, work, iters, token);

        Task<AlgorithmResult> MhInvoker(int iters, CancellationToken token) =>
            algoMh.RunAsync(args.CipherPath, args.BigramsPath, work, iters, token);
    }
}