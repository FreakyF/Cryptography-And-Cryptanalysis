using Task01.Application.Abstractions;
using Task01.Application.Models;
using Task01.Domain.Abstractions;

namespace Task01.Application.Services;

public sealed class CipherOrchestrator(
    IFileService fileService,
    IKeyProvider keyProvider,
    ITextNormalizer textNormalizer,
    IAlphabetBuilder alphabetBuilder,
    ICaesarCipher cipher)
    : ICipherOrchestrator
{
    public async Task<ProcessingResult> RunAsync(Arguments args)
    {
        try
        {
            var rawInput = await fileService.ReadAllTextAsync(args.InputFilePath).ConfigureAwait(false);

            var normalized = textNormalizer.Normalize(rawInput);

            var alphabet = alphabetBuilder.BuildAlphabet(normalized);

            var key = await keyProvider.GetKeyAsync(args.KeyFilePath).ConfigureAwait(false);

            var outputText = args.Operation == Operation.Encrypt
                ? cipher.Encrypt(normalized, alphabet, key)
                : cipher.Decrypt(normalized, alphabet, key);

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