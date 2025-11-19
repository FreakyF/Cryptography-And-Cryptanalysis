using System.Runtime.CompilerServices;
using Task03.Application.Abstractions;
using Task03.Application.Models;
using Task03.Domain.Abstractions;

namespace Task03.Application.Services;

public sealed class CipherOrchestrator(
    IFileService fileService,
    IKeyService keyService,
    ITextNormalizer textNormalizer,
    ISubstitutionCipher cipher,
    IHeuristicAnalyzer heuristicAnalyzer)
    : ICipherOrchestrator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

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

                fileService.WriteAllText(args.OutputFilePath, outputText);

                var keyPath = BuildSiblingPath(args.OutputFilePath, "cipher_key.txt");
                fileService.WriteAllText(keyPath, permutation);

                return new ProcessingResult(0, null);
            }

            var cipherPayload = ExtractCipherPayload(rawInput);
            var normalizedCipher = textNormalizer.Normalize(cipherPayload);

            if (normalizedCipher.Length == 0)
            {
                fileService.WriteAllText(args.OutputFilePath, string.Empty);
                return new ProcessingResult(0, null);
            }

            var bigramTableText = ReadReferenceText(args);

            var heuristicResult = heuristicAnalyzer.Analyze(normalizedCipher, bigramTableText, Alphabet);

            fileService.WriteAllText(args.OutputFilePath, heuristicResult.PlainText);

            var outKeyPath = BuildSiblingPath(args.OutputFilePath, "output_key.txt");
            fileService.WriteAllText(outKeyPath, heuristicResult.Permutation);

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
    private static string BuildSiblingPath(string basePath, string fileName)
    {
        var dir = Path.GetDirectoryName(basePath);
        return string.IsNullOrEmpty(dir) ? fileName : Path.Combine(dir, fileName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ExtractCipherPayload(string rawInput)
    {
        if (string.IsNullOrEmpty(rawInput))
        {
            return string.Empty;
        }

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
        {
            return fileService.ReadAllText(args.ReferenceFilePath);
        }

        var defaultPath = Path.Combine(AppContext.BaseDirectory, "Samples", "bigrams.txt");
        return File.Exists(defaultPath)
            ? fileService.ReadAllText(defaultPath)
            : throw new FileNotFoundException("Bigram table not found", defaultPath);
    }
}