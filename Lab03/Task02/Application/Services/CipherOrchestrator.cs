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

    /// <summary>
    ///     Executes the substitution cipher workflow by handling I/O, normalization, key retrieval, and encryption or
    ///     decryption.
    /// </summary>
    /// <param name="args">The prepared arguments describing input, output, key locations, and the desired operation.</param>
    /// <returns>A processing result indicating success or the encountered error.</returns>
    public async Task<ProcessingResult> RunAsync(Arguments args)
    {
        try
        {
            var rawInput = await fileService.ReadAllTextAsync(args.InputFilePath).ConfigureAwait(false);
            if (args.Operation == Operation.Encrypt)
            {
                var normalized = textNormalizer.Normalize(rawInput);
                var permutation = keyService.CreatePermutation(Alphabet);
                var outputText = cipher.Encrypt(normalized, Alphabet, permutation);
                var builder = new StringBuilder(permutation.Length + Environment.NewLine.Length + outputText.Length);
                builder.AppendLine(permutation);
                builder.Append(outputText);

                await fileService.WriteAllTextAsync(args.OutputFilePath, builder.ToString()).ConfigureAwait(false);

                return new ProcessingResult(0, null);
            }

            var cipherPayload = ExtractCipherPayload(rawInput);
            var normalizedCipher = textNormalizer.Normalize(cipherPayload);
            if (normalizedCipher.Length == 0)
            {
                await fileService.WriteAllTextAsync(args.OutputFilePath, string.Empty).ConfigureAwait(false);
                return new ProcessingResult(0, null);
            }

            var referenceText = await ReadReferenceTextAsync(args).ConfigureAwait(false);
            var normalizedReference = textNormalizer.Normalize(referenceText);
            var heuristicResult = heuristicAnalyzer.Analyze(normalizedCipher, normalizedReference, Alphabet);

            var outputBuilder = new StringBuilder(
                heuristicResult.Permutation.Length + Environment.NewLine.Length + heuristicResult.PlainText.Length);
            outputBuilder.AppendLine(heuristicResult.Permutation);
            outputBuilder.Append(heuristicResult.PlainText);

            await fileService.WriteAllTextAsync(args.OutputFilePath, outputBuilder.ToString()).ConfigureAwait(false);

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

    /// <summary>
    ///     Extracts the cipher text portion from the input while discarding any persisted permutation header, if present.
    /// </summary>
    /// <param name="rawInput">The raw file contents that may begin with a persisted permutation line.</param>
    /// <returns>The cipher text content without the persisted permutation header.</returns>
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

    /// <summary>Loads the reference corpus used to build the heuristic language model.</summary>
    /// <param name="args">The command arguments potentially containing a reference file path.</param>
    /// <returns>The text that will serve as the basis for the bigram probabilities.</returns>
    private async Task<string> ReadReferenceTextAsync(Arguments args)
    {
        if (!string.IsNullOrWhiteSpace(args.ReferenceFilePath))
        {
            return await fileService.ReadAllTextAsync(args.ReferenceFilePath).ConfigureAwait(false);
        }

        var defaultPath = Path.Combine(AppContext.BaseDirectory, "Samples", "plaintext.txt");
        if (File.Exists(defaultPath))
        {
            return await fileService.ReadAllTextAsync(defaultPath).ConfigureAwait(false);
        }

        throw new FileNotFoundException("Reference corpus not found", defaultPath);
    }
}