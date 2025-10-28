using Task01.Application.Abstractions;
using Task01.Application.Models;
using Task01.Domain.Abstractions;

namespace Task01.Application.Services;

public sealed class CipherOrchestrator(
    IFileService fileService,
    IKeyService keyService,
    ITextNormalizer textNormalizer,
    ICaesarCipher cipher)
    : ICipherOrchestrator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>Executes the Caesar cipher workflow by handling I/O, normalization, key retrieval, and encryption or decryption.</summary>
    /// <param name="args">The prepared arguments describing input, output, key locations, and the desired operation.</param>
    /// <returns>A processing result indicating success or the encountered error.</returns>
    public async Task<ProcessingResult> RunAsync(Arguments args)
    {
        try
        {
            var rawInput = await fileService.ReadAllTextAsync(args.InputFilePath).ConfigureAwait(false);

            var normalized = textNormalizer.Normalize(rawInput);

            var key = await keyService.GetKeyAsync(args.KeyFilePath).ConfigureAwait(false);

            var outputText = args.Operation == Operation.Encrypt
                ? cipher.Encrypt(normalized, Alphabet, key)
                : cipher.Decrypt(normalized, Alphabet, key);

            await fileService.WriteAllTextAsync(args.OutputFilePath, outputText).ConfigureAwait(false);

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