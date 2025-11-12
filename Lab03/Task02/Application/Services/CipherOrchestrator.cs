using Task02.Application.Abstractions;
using Task02.Application.Models;
using Task02.Domain.Abstractions;

namespace Task02.Application.Services;

public sealed class CipherOrchestrator(
    IFileService fileService,
    IKeyService keyService,
    ITextNormalizer textNormalizer,
    ISubstitutionCipher cipher)
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
            }
            else
            {
                var permutation = keyService.ExtractPermutation(rawInput, Alphabet, out var cipherSection);

                var normalized = textNormalizer.Normalize(cipherSection);

                var plainText = cipher.Decrypt(normalized, Alphabet, permutation);

                await fileService.WriteAllTextAsync(args.OutputFilePath, plainText).ConfigureAwait(false);
            }

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
}