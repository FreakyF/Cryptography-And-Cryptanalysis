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

    /// <summary>Runs the encryption or decryption routine, coordinating file IO, key management, and heuristic analysis.</summary>
    /// <param name="args">The parsed arguments describing the requested operation and file paths.</param>
    /// <returns>The processing result containing success status and optional message.</returns>
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
            if (heuristicAnalyzer is IConfigurableIterations cfg)
                cfg.SetIterations(args.Iterations);

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

    /// <summary>Builds a file path located in the same directory as the supplied base path.</summary>
    /// <param name="basePath">The path whose directory will be reused.</param>
    /// <param name="fileName">The file name to append within that directory.</param>
    /// <returns>The combined path for the sibling file.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BuildSiblingPath(string basePath, string fileName)
    {
        var dir = Path.GetDirectoryName(basePath);
        return string.IsNullOrEmpty(dir) ? fileName : Path.Combine(dir, fileName);
    }

    /// <summary>Attempts to strip the persisted key header from the cipher payload, falling back to the raw input.</summary>
    /// <param name="rawInput">The raw contents read from the cipher file.</param>
    /// <returns>The cipher text portion without the embedded key if extraction succeeds; otherwise the original text.</returns>
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

    /// <summary>Loads the reference bigram table from the provided path or from the default samples directory.</summary>
    /// <param name="args">The parsed arguments that may specify a custom reference file.</param>
    /// <returns>The reference text containing bigram frequencies.</returns>
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