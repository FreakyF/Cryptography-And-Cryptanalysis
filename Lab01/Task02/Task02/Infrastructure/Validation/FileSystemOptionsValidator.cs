using Task02.Application.Abstractions;
using Task02.Application.Models;

namespace Task02.Infrastructure.Validation;

#pragma warning disable S2325
public sealed class FileSystemOptionsValidator : IOptionsValidator
{
    public IReadOnlyList<string> Validate(AppOptions options)
    {
        if (options.ShowHelp) return [];

        var errors = new List<string>();
        ValidateInputPath(options.InputPath, errors);
        ValidateKeyPath(options.KeyPath, errors);
        ValidateOutputPath(options.OutputPath, options.InputPath, errors);
        return errors;
    }

    private static void ValidateInputPath(string? inputPath, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
            errors.Add($"Input file not found: {inputPath}");
    }

    private static void ValidateKeyPath(string? keyPath, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(keyPath) || !File.Exists(keyPath))
            errors.Add($"Key file not found: {keyPath}");
    }

    private static void ValidateOutputPath(string? outputPath, string? inputPath, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            errors.Add("Missing output path.");
            return;
        }

        if (Directory.Exists(outputPath))
            errors.Add("Output path points to a directory.");

        var inFull = GetFullPathOrNull(inputPath);
        var outFull = GetFullPathOrNull(outputPath);

        if (inFull is not null && outFull is not null && PathsEqual(inFull, outFull))
            errors.Add("Input and output paths must differ.");

        if (outFull is null || Path.GetDirectoryName(outFull) is null)
            errors.Add("Output path is invalid.");
    }

    private static string? GetFullPathOrNull(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        try
        {
            return Path.GetFullPath(path);
        }
        catch
        {
            return null;
        }
    }

    private static bool PathsEqual(string a, string b)
    {
        var cmp = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        return cmp.Equals(a, b);
    }
}