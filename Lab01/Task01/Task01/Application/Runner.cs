using Task01.Application.Abstractions;
using Task01.Application.Models;
using Task01.Domain.Enums;

namespace Task01.Application;

public sealed class Runner(
    IKeyLoader keyLoader,
    IFileReader reader,
    IFileWriter writer,
    ITextNormalizer normalizer,
    ISubstitutionCipher cipher)
    : IRunner
{
    private readonly IKeyLoader _keyLoader = keyLoader ?? throw new ArgumentNullException(nameof(keyLoader));
    private readonly IFileReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    private readonly IFileWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    private readonly ITextNormalizer _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
    private readonly ISubstitutionCipher _cipher = cipher ?? throw new ArgumentNullException(nameof(cipher));

    /// <summary>Orchestrates reading, normalizing, encrypting or decrypting, and writing text according to the options.</summary>
    /// <param name="options">The application options describing file paths and the operation mode.</param>
    /// <returns>Zero when the workflow succeeds; otherwise one when an error occurs.</returns>
    public int Run(AppOptions options)
    {
        try
        {
            var key = _keyLoader.Load(options.KeyPath!);

            var raw = _reader.ReadAll(options.InputPath!);
            var normalized = _normalizer.Normalize(raw);

            var result = options.Mode switch
            {
                OperationMode.Encrypt => _cipher.Encrypt(normalized, key),
                OperationMode.Decrypt => _cipher.Decrypt(normalized, key),
                OperationMode.Unspecified => throw new InvalidOperationException("Mode not set."),
                _ => throw new ArgumentOutOfRangeException(nameof(options), $"Unknown mode: {options.Mode}.")
            };


            _writer.WriteAll(options.OutputPath!, result);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }
    }
}