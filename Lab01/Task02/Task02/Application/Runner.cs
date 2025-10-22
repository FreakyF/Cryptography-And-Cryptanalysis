using Task02.Application.Abstractions;
using Task02.Application.Analysis;
using Task02.Application.Models;
using Task02.Domain.Enums;

namespace Task02.Application;

public sealed class Runner(
    IKeyLoader keyLoader,
    IFileReader reader,
    IFileWriter writer,
    ITextNormalizer normalizer,
    ISubstitutionCipher cipher,
    INGramCounter ngramCounter)
    : IRunner
{
    private readonly IKeyLoader _keyLoader = keyLoader ?? throw new ArgumentNullException(nameof(keyLoader));
    private readonly IFileReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    private readonly IFileWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    private readonly ITextNormalizer _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
    private readonly ISubstitutionCipher _cipher = cipher ?? throw new ArgumentNullException(nameof(cipher));
    private readonly INGramCounter _ngrams = ngramCounter ?? throw new ArgumentNullException(nameof(ngramCounter));

    /// <summary>Coordinates loading inputs, performing cipher or n-gram operations, and writing the resulting outputs.</summary>
    /// <param name="options">The application options containing file paths, mode selection, and report destinations.</param>
    /// <returns>Zero if all requested operations complete successfully; otherwise one when an error is encountered.</returns>
    public int Run(AppOptions options)
    {
        try
        {
            var raw = _reader.ReadAll(options.InputPath!);
            var normalized = _normalizer.Normalize(raw);

            var isCipher = options.Mode is not OperationMode.Unspecified;
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

    /// <summary>Generates and writes an n-gram frequency report when an output path is provided.</summary>
    /// <param name="normalized">The normalized text that should be analyzed.</param>
    /// <param name="n">The n-gram size requested for the analysis.</param>
    /// <param name="outPath">The destination file path for the generated report.</param>
    private void GenerateIfRequested(string normalized, int n, string? outPath)
    {
        if (string.IsNullOrWhiteSpace(outPath)) return;
        var counts = _ngrams.Count(normalized, n);
        var report = NGramReportBuilder.Build(counts);
        _writer.WriteAll(outPath, report);
    }
}