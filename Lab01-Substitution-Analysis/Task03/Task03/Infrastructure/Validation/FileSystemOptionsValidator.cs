using Task03.Application.Abstractions;
using Task03.Application.Models;
using Task03.Domain.Enums;

namespace Task03.Infrastructure.Validation;

#pragma warning disable S2325
public sealed class FileSystemOptionsValidator : IOptionsValidator
{
    /// <summary>Checks that required files exist and output destinations are valid for the chosen operations.</summary>
    /// <param name="options">The parsed application options providing paths and requested tasks.</param>
    /// <returns>A collection containing descriptions of any file system validation errors.</returns>
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

    /// <summary>Validates an output path ensuring it differs from the input and can be created, annotating errors with labels.</summary>
    /// <param name="outputPath">The candidate output file path.</param>
    /// <param name="inputPath">The input path used to detect conflicts.</param>
    /// <param name="errors">The collection that accumulates validation error messages.</param>
    /// <param name="label">An optional label identifying the option that supplied the path.</param>
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

    /// <summary>Converts the provided path to its absolute representation or returns null when conversion fails.</summary>
    /// <param name="path">The path value to normalize.</param>
    /// <returns>The absolute path string if obtainable; otherwise null.</returns>
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

    /// <summary>Determines whether two file system paths refer to the same location using platform-aware comparison.</summary>
    /// <param name="a">The first normalized path.</param>
    /// <param name="b">The second normalized path.</param>
    /// <returns><see langword="true"/> when both paths identify the same location; otherwise <see langword="false"/>.</returns>
    private static bool PathsEqual(string a, string b)
    {
        var cmp = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        return cmp.Equals(a, b);
    }

    /// <summary>Formats an optional label prefix for validation messages.</summary>
    /// <param name="label">The command-line flag label associated with the message.</param>
    /// <returns>An empty string when no label is supplied; otherwise the label followed by a separator.</returns>
    private static string Prefix(string? label) => string.IsNullOrEmpty(label) ? "" : $"{label}: ";
}