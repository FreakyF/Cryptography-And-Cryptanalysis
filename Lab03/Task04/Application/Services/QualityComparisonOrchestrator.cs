using Task04.Application.Models;
using Task04.Domain.Abstractions;
using Task04.Domain.Models;
using Task04.Infrastructure.Abstractions;
using Task04.Infrastructure.Services;

namespace Task04.Application.Services;

public sealed class QualityComparisonOrchestrator(
    IAlgorithmRunner algoMh,
    IAlgorithmRunner algoSa,
    IQualityEvaluator evaluator)
{
    public async Task RunAsync(QualityComparisonArgs args, CancellationToken ct = default)
    {
        var work = TempFiles.EnsureDir(args.WorkDir, ".task04");

        var refPlain = await File.ReadAllTextAsync(args.PlainRefPath, ct).ConfigureAwait(false);

        string? trueKey = null;
        if (!string.IsNullOrWhiteSpace(args.TrueKeyPath) && File.Exists(args.TrueKeyPath!))
            trueKey = await File.ReadAllTextAsync(args.TrueKeyPath!, ct).ConfigureAwait(false);

        var mh = await algoMh.RunAsync(args.CipherPath, args.BigramsPath, work);

        var sa = await algoSa.RunAsync(args.CipherPath, args.BigramsPath, work);

        var (mhTextAcc, mhKeyAcc) = evaluator.Evaluate(mh.DecryptedText, refPlain, mh.RecoveredKey, trueKey);
        var (saTextAcc, saKeyAcc) = evaluator.Evaluate(sa.DecryptedText, refPlain, sa.RecoveredKey, trueKey);

        var rows = new List<QualityMetrics>
        {
            new("MH (Task02)", mhTextAcc, mhKeyAcc),
            new("SA (Task03)", saTextAcc, saKeyAcc)
        };

        ConsoleReport.PrintQualityTable(rows);
    }
}