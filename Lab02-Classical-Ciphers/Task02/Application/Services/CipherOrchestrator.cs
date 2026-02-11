using Task02.Application.Abstractions;
using Task02.Application.Models;
using Task02.Domain.Abstractions;

namespace Task02.Application.Services;

public sealed class CipherOrchestrator(
    IFileService fileService,
    IKeyService keyService,
    ITextNormalizer normalizer,
    ICaesarCipher cipher,
    IBruteForceAttack bruteForce)
    : ICipherOrchestrator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>Runs the requested operation by coordinating I/O, normalization, key retrieval, and cipher or brute-force processing.</summary>
    /// <param name="args">The prepared arguments describing input, output, key locations, and the desired operation.</param>
    /// <returns>A processing result indicating success or the encountered error.</returns>
    public async Task<ProcessingResult> RunAsync(Arguments args)
    {
        try
        {
            return args.Operation switch
            {
                Operation.Encrypt => await RunEncryptAsync(args).ConfigureAwait(false),
                Operation.Decrypt => await RunDecryptAsync(args).ConfigureAwait(false),
                Operation.BruteForce => await RunBruteForceAsync(args).ConfigureAwait(false),
                _ => new ProcessingResult(1, "Unsupported operation")
            };
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

    /// <summary>Encrypts the normalized input text and writes the ciphertext to the output file.</summary>
    /// <param name="args">The arguments containing file locations and key information.</param>
    /// <returns>A processing result indicating whether the encryption succeeded.</returns>
    private async Task<ProcessingResult> RunEncryptAsync(Arguments args)
    {
        var raw = await fileService.ReadAllTextAsync(args.InputFilePath).ConfigureAwait(false);
        var norm = normalizer.Normalize(raw);

        var key = await keyService.GetKeyAsync(args.KeyFilePath!).ConfigureAwait(false);

        var output = cipher.Encrypt(norm, Alphabet, key);

        await fileService.WriteAllTextAsync(args.OutputFilePath, output).ConfigureAwait(false);

        return new ProcessingResult(0, null);
    }

    /// <summary>Decrypts the normalized input text and writes the plaintext to the output file.</summary>
    /// <param name="args">The arguments containing file locations and key information.</param>
    /// <returns>A processing result indicating whether the decryption succeeded.</returns>
    private async Task<ProcessingResult> RunDecryptAsync(Arguments args)
    {
        var raw = await fileService.ReadAllTextAsync(args.InputFilePath).ConfigureAwait(false);
        var norm = normalizer.Normalize(raw);

        var key = await keyService.GetKeyAsync(args.KeyFilePath!).ConfigureAwait(false);

        var output = cipher.Decrypt(norm, Alphabet, key);

        await fileService.WriteAllTextAsync(args.OutputFilePath, output).ConfigureAwait(false);

        return new ProcessingResult(0, null);
    }

    /// <summary>Performs a brute-force analysis of the ciphertext and writes the best candidate plaintext to the output file.</summary>
    /// <param name="args">The arguments containing file locations and operation details.</param>
    /// <returns>A processing result including diagnostics about the brute-force outcome.</returns>
    private async Task<ProcessingResult> RunBruteForceAsync(Arguments args)
    {
        var raw = await fileService.ReadAllTextAsync(args.InputFilePath).ConfigureAwait(false);
        var norm = normalizer.Normalize(raw);

        var result = bruteForce.BreakCipher(norm);

        await fileService.WriteAllTextAsync(args.OutputFilePath, result.Plaintext).ConfigureAwait(false);

        var msg = $"key={result.Key} chi2={result.ChiSquare:F4} english={result.LooksEnglish}";

        return new ProcessingResult(0, msg);
    }
}