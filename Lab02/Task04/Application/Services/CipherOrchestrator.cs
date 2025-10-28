using Task04.Application.Abstractions;
using Task04.Application.Models;
using Task04.Domain.Abstractions;

namespace Task04.Application.Services;

public sealed class CipherOrchestrator(
    IFileService fileService,
    IKeyService keyService,
    ITextNormalizer textNormalizer,
    IAffineCipher cipher)
    : ICipherOrchestrator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public async Task<ProcessingResult> RunAsync(Arguments args)
    {
        try
        {
            var rawInput = await fileService.ReadAllTextAsync(args.InputFilePath).ConfigureAwait(false);

            var normalized = textNormalizer.Normalize(rawInput);

            var (a, b) = await keyService.GetKeyAsync(args.KeyFilePath).ConfigureAwait(false);

            var outputText = args.Operation == Operation.Encrypt
                ? cipher.Encrypt(normalized, Alphabet, a, b)
                : cipher.Decrypt(normalized, Alphabet, a, b);

            await fileService.WriteAllTextAsync(args.OutputFilePath, outputText).ConfigureAwait(false);

            return new ProcessingResult(0, null);
        }
        catch (FormatException)
        {
            return new ProcessingResult(3, "Invalid key");
        }
        catch (InvalidOperationException)
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