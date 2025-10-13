using Task03.Application.Abstractions;
using Task03.Application.Analysis;
using Task03.Application.Models;
using Task03.Domain.Enums;

namespace Task03.Application;

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

    private readonly INGramCounter _ngrams = analysis.NGramCounter
                                             ?? throw new ArgumentNullException(nameof(analysis));

    private readonly IReferenceLoader _refLoader = analysis.ReferenceLoader;
    private readonly IChiSquareCalculator _chi2 = analysis.ChiSquare;

    public int Run(AppOptions options)
    {
        try
        {
            var raw = _reader.ReadAll(options.InputPath!);
            var normalized = _normalizer.Normalize(raw);

            var isCipher = options.Mode is not OperationMode.Unspecified;
            var isChi2 = options.ComputeChiSquare;
            var isBuild = options.AnyRefBuildRequested;

            if (isCipher)
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

            if (isChi2)
            {
                var reference = _refLoader.Load(options.ReferencePath!);
                var n = options.ReferenceOrder!.Value;
                var t = _chi2.Compute(normalized, n, reference);
                Console.WriteLine(t.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return 0;
            }

            if (isBuild)
            {
                BuildRefIfRequested(normalized, 1, options.B1OutputPath);
                BuildRefIfRequested(normalized, 2, options.B2OutputPath);
                BuildRefIfRequested(normalized, 3, options.B3OutputPath);
                BuildRefIfRequested(normalized, 4, options.B4OutputPath);
                return 0;
            }

            // n-gram counts
            GenerateIfRequested(normalized, 1, options.G1OutputPath);
            GenerateIfRequested(normalized, 2, options.G2OutputPath);
            GenerateIfRequested(normalized, 3, options.G3OutputPath);
            GenerateIfRequested(normalized, 4, options.G4OutputPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }
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