using Task04.Application.Abstractions;
using Task04.Application.Models;
using Task04.Domain.Abstractions;

namespace Task04.Application.Services;

public sealed class CipherOrchestrator(
    IFileService fileService,
    IKeyService keyProvider,
    ITextNormalizer textNormalizer,
    IAffineCipher cipher,
    IBruteForceAttack brute)
    : ICipherOrchestrator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>Runs the requested operation and handles expected application-level failures.</summary>
    /// <param name="args">The parsed command-line arguments describing the operation.</param>
    /// <returns>The result detailing the exit code and optional message.</returns>
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

    /// <summary>Encrypts the normalized input text and persists the ciphertext to the output file.</summary>
    /// <param name="args">The arguments containing the paths and affine key.</param>
    /// <returns>A successful processing result when encryption completes.</returns>
    private async Task<ProcessingResult> RunEncryptAsync(Arguments args)
    {
        var raw = await fileService.ReadAllTextAsync(args.InputFilePath).ConfigureAwait(false);
        var norm = textNormalizer.Normalize(raw);
        var (a, b) = await keyProvider.GetKeyAsync(args.KeyFilePath!).ConfigureAwait(false);
        var output = cipher.Encrypt(norm, Alphabet, a, b);
        await fileService.WriteAllTextAsync(args.OutputFilePath, output).ConfigureAwait(false);
        return new ProcessingResult(0, null);
    }

    /// <summary>Decrypts the normalized input text and writes the plaintext to the output file.</summary>
    /// <param name="args">The arguments containing the paths and affine key.</param>
    /// <returns>A successful processing result when decryption completes.</returns>
    private async Task<ProcessingResult> RunDecryptAsync(Arguments args)
    {
        var raw = await fileService.ReadAllTextAsync(args.InputFilePath).ConfigureAwait(false);
        var norm = textNormalizer.Normalize(raw);
        var (a, b) = await keyProvider.GetKeyAsync(args.KeyFilePath!).ConfigureAwait(false);
        var output = cipher.Decrypt(norm, Alphabet, a, b);
        await fileService.WriteAllTextAsync(args.OutputFilePath, output).ConfigureAwait(false);
        return new ProcessingResult(0, null);
    }

    /// <summary>Attempts every valid key combination to recover plaintext and stores the best candidate.</summary>
    /// <param name="args">The arguments containing the input and output paths.</param>
    /// <returns>A successful processing result whose message summarizes the discovered key.</returns>
    private async Task<ProcessingResult> RunBruteForceAsync(Arguments args)
    {
        var raw = await fileService.ReadAllTextAsync(args.InputFilePath).ConfigureAwait(false);
        var norm = textNormalizer.Normalize(raw);
        var r = brute.BreakCipher(norm);
        await fileService.WriteAllTextAsync(args.OutputFilePath, r.Plaintext).ConfigureAwait(false);
        var msg = $"a={r.A} b={r.B} chi2={r.ChiSquare:F4} english={r.LooksEnglish}";
        return new ProcessingResult(0, msg);
    }
}