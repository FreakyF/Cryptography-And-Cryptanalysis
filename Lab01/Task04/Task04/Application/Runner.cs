using System.Globalization;
using Task04.Application.Abstractions;
using Task04.Application.Analysis;
using Task04.Application.Models;
using Task04.Domain.Enums;

namespace Task04.Application;

public sealed class Runner(
    IKeyLoader keyLoader,
    IFileReader reader,
    IFileWriter writer,
    ITextNormalizer normalizer,
    ISubstitutionCipher cipher,
    AnalysisServices analysis)
    : IRunner
{
    private readonly IKeyLoader _keyLoader = keyLoader ?? throw new ArgumentNullException(nameof(keyLoader));
    private readonly IFileReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    private readonly IFileWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    private readonly ITextNormalizer _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
    private readonly ISubstitutionCipher _cipher = cipher ?? throw new ArgumentNullException(nameof(cipher));

    private readonly INGramCounter _ngrams =
        analysis.NGramCounter ?? throw new ArgumentNullException(nameof(analysis));

    private readonly IReferenceLoader _refLoader = analysis.ReferenceLoader;
    private readonly IChiSquareCalculator _chi2 = analysis.ChiSquare;

    public int Run(AppOptions options)
    {
        try
        {
            var normalized = _normalizer.Normalize(_reader.ReadAll(options.InputPath!));

            if (options.Mode is not OperationMode.Unspecified)
                return ProcessCipher(options, normalized);

            if (options.ComputeChiSquare)
                return ProcessChiSquare(options, normalized);

            return options.AnyRefBuildRequested
                ? ProcessBuild(options, normalized)
                : ProcessCounts(options, normalized);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }
    }

    private int ProcessCipher(AppOptions options, string normalized)
    {
        var key = _keyLoader.Load(options.KeyPath!);
        var result = options.Mode switch
        {
            OperationMode.Encrypt => _cipher.Encrypt(normalized, key),
            OperationMode.Decrypt => _cipher.Decrypt(normalized, key),
            OperationMode.Unspecified => throw new InvalidOperationException("Mode not set."),
            _ => throw new ArgumentOutOfRangeException(nameof(options), $"Unknown mode: {options.Mode}.")
        };
        _writer.WriteAll(options.OutputPath!, result);
        return 0;
    }

    private int ProcessChiSquare(AppOptions options, string normalized)
    {
        if (options.SampleLength is { } nlen && nlen < normalized.Length)
            normalized = normalized[..nlen];

        var reference = _refLoader.Load(options.ReferencePath!);
        var n = options.ReferenceOrder!.Value;

        var exclude = ParseExclusions(options.ExcludeCsv, n);
        var csOpts = new ChiSquareOptions(exclude, options.MinExpected);

        var t = _chi2.Compute(normalized, n, reference, csOpts);
        Console.WriteLine(t.ToString(CultureInfo.InvariantCulture));
        return 0;
    }

    private int ProcessBuild(AppOptions options, string normalized)
    {
        BuildRefIfRequested(normalized, 1, options.B1OutputPath);
        BuildRefIfRequested(normalized, 2, options.B2OutputPath);
        BuildRefIfRequested(normalized, 3, options.B3OutputPath);
        BuildRefIfRequested(normalized, 4, options.B4OutputPath);
        return 0;
    }

    private int ProcessCounts(AppOptions options, string normalized)
    {
        GenerateIfRequested(normalized, 1, options.G1OutputPath);
        GenerateIfRequested(normalized, 2, options.G2OutputPath);
        GenerateIfRequested(normalized, 3, options.G3OutputPath);
        GenerateIfRequested(normalized, 4, options.G4OutputPath);
        return 0;
    }

    private static HashSet<string> ParseExclusions(string? csv, int n)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(csv)) return set;

        foreach (var tok in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var g = tok.Trim().ToUpperInvariant();
            if (g.Length == n && g.All(c => c is >= 'A' and <= 'Z'))
                set.Add(g);
        }

        return set;
    }

    private void GenerateIfRequested(string normalized, int n, string? outPath)
    {
        if (string.IsNullOrWhiteSpace(outPath)) return;
        var counts = _ngrams.Count(normalized, n);
        var report = NGramReportBuilder.Build(counts);
        _writer.WriteAll(outPath, report);
    }

    private void BuildRefIfRequested(string normalized, int n, string? outPath)
    {
        if (string.IsNullOrWhiteSpace(outPath)) return;
        var counts = _ngrams.Count(normalized, n);
        var report = ReferenceReportBuilder.BuildProbabilities(counts);
        _writer.WriteAll(outPath, report);
    }
}