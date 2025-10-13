using Task04.Application.Abstractions;
using Task04.Application.Models;
using Task04.Domain.Enums;

namespace Task04.Infrastructure.Validation;

#pragma warning disable S2325
public sealed class FileSystemOptionsValidator : IOptionsValidator
{
    public IReadOnlyList<string> Validate(AppOptions options)
    {
        if (options.ShowHelp) return [];

        var errors = new List<string>();
        if (options.InputPath is null || !File.Exists(options.InputPath))
            errors.Add($"Input file not found: {options.InputPath}");

        var hasCipher = options.Mode is not OperationMode.Unspecified;
        var hasNgrams = options.AnyNGramRequested;
        var hasChi2 = options.ComputeChiSquare || options.ReferenceOrder is not null;
        var hasBuild = options.AnyRefBuildRequested;

        if (hasCipher)
        {
            if (options.KeyPath is null || !File.Exists(options.KeyPath))
                errors.Add($"Key file not found: {options.KeyPath}");
            ValidateOutputPath(options.OutputPath, options.InputPath, errors);
        }

        if (hasNgrams)
        {
            ValidateOutputPath(options.G1OutputPath, options.InputPath, errors, "-g1");
            ValidateOutputPath(options.G2OutputPath, options.InputPath, errors, "-g2");
            ValidateOutputPath(options.G3OutputPath, options.InputPath, errors, "-g3");
            ValidateOutputPath(options.G4OutputPath, options.InputPath, errors, "-g4");
        }

        if (hasBuild)
        {
            ValidateOutputPath(options.B1OutputPath, options.InputPath, errors, "-b1");
            ValidateOutputPath(options.B2OutputPath, options.InputPath, errors, "-b2");
            ValidateOutputPath(options.B3OutputPath, options.InputPath, errors, "-b3");
            ValidateOutputPath(options.B4OutputPath, options.InputPath, errors, "-b4");
        }

        if (!hasChi2) return errors;
        var rPath = options.ReferencePath;
        if (rPath is null || !File.Exists(rPath))
            errors.Add($"Reference file not found: {rPath}");

        return errors;
    }

    private static void ValidateOutputPath(string? outputPath, string? inputPath, List<string> errors,
        string? label = null)
    {
        if (string.IsNullOrWhiteSpace(outputPath)) return;

        if (Directory.Exists(outputPath))
            errors.Add($"{Prefix(label)}Output path points to a directory.");

        var inFull = GetFullPathOrNull(inputPath);
        var outFull = GetFullPathOrNull(outputPath);

        if (inFull is not null && outFull is not null && PathsEqual(inFull, outFull))
            errors.Add($"{Prefix(label)}Input and output paths must differ.");

        if (outFull is null || Path.GetDirectoryName(outFull) is null)
            errors.Add($"{Prefix(label)}Output path is invalid.");
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

    private static string Prefix(string? label) => string.IsNullOrEmpty(label) ? "" : $"{label}: ";
}