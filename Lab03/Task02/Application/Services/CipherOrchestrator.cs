using System.Runtime.CompilerServices;
using Task02.Application.Abstractions;
using Task02.Application.Models;
using Task02.Domain.Abstractions;

namespace Task02.Application.Services;

public sealed class CipherOrchestrator(
    IFileService fileService,
    IKeyService keyService,
    ITextNormalizer textNormalizer,
    ISubstitutionCipher cipher,
    IHeuristicAnalyzer heuristicAnalyzer)
    : ICipherOrchestrator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly string NewLine = Environment.NewLine;
    private static readonly int NewLineLen = NewLine.Length;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ProcessingResult Run(Arguments args)
    {
        try
        {
            var rawInput = fileService.ReadAllText(args.InputFilePath);

            if (args.Operation == Operation.Encrypt)
            {
                var normalized = textNormalizer.Normalize(rawInput);
                var permutation = keyService.CreatePermutation(Alphabet);
                var outputText = cipher.Encrypt(normalized, Alphabet, permutation);

                var payload = string.Create(
                    permutation.Length + NewLineLen + outputText.Length,
                    (permutation, outputText),
                    static (dst, s) =>
                    {
                        s.permutation.AsSpan().CopyTo(dst);
                        dst = dst[s.permutation.Length..];
                        NewLine.AsSpan().CopyTo(dst);
                        dst = dst[NewLineLen..];
                        s.outputText.AsSpan().CopyTo(dst);
                    });

                fileService.WriteAllText(args.OutputFilePath, payload);
                return new ProcessingResult(0, null);
            }

            var cipherPayload = ExtractCipherPayload(rawInput);
            var normalizedCipher = textNormalizer.Normalize(cipherPayload);

            if (normalizedCipher.Length == 0)
            {
                fileService.WriteAllText(args.OutputFilePath, string.Empty);
                return new ProcessingResult(0, null);
            }

            // UWAGA: teraz wczytujemy tabelę bigramów (bigrams.txt),
            // bez normalizacji – surowy tekst trafia do analizatora.
            var bigramTableText = ReadReferenceText(args);

            var heuristicResult = heuristicAnalyzer.Analyze(normalizedCipher, bigramTableText, Alphabet);

            var output = string.Create(
                heuristicResult.Permutation.Length + NewLineLen + heuristicResult.PlainText.Length,
                heuristicResult,
                static (dst, hr) =>
                {
                    hr.Permutation.AsSpan().CopyTo(dst);
                    dst = dst[hr.Permutation.Length..];
                    NewLine.AsSpan().CopyTo(dst);
                    dst = dst[NewLineLen..];
                    hr.PlainText.AsSpan().CopyTo(dst);
                });

            fileService.WriteAllText(args.OutputFilePath, output);
            return new ProcessingResult(0, null);
        }
        catch (FormatException)
        {
            return new ProcessingResult(3, "Invalid key");
        }
        catch (FileNotFoundException)
        {
            return new ProcessingResult(2, "File error");
        }
        catch (DirectoryNotFoundException)
        {
            return new ProcessingResult(2, "File error");
        }
        catch (UnauthorizedAccessException)
        {
            return new ProcessingResult(2, "File error");
        }
        catch (IOException)
        {
            return new ProcessingResult(2, "File error");
        }
        catch (Exception)
        {
            return new ProcessingResult(99, "Unexpected error");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ExtractCipherPayload(string rawInput)
    {
        if (string.IsNullOrEmpty(rawInput)) return string.Empty;

        try
        {
            keyService.ExtractPermutation(rawInput, Alphabet, out var cipherSection);
            return cipherSection;
        }
        catch (FormatException)
        {
            return rawInput;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private string ReadReferenceText(Arguments args)
    {
        if (!string.IsNullOrWhiteSpace(args.ReferenceFilePath))
            return fileService.ReadAllText(args.ReferenceFilePath);

        var defaultPath = Path.Combine(AppContext.BaseDirectory, "Samples", "bigrams.txt");
        if (File.Exists(defaultPath))
            return fileService.ReadAllText(defaultPath);

        throw new FileNotFoundException("Bigram table not found", defaultPath);
    }
}