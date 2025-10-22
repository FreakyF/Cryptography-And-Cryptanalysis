using Task01.Application.Abstractions;
using Task01.Application.Models;

namespace Task01.Infrastructure.Validation;

#pragma warning disable S2325
public sealed class FileSystemOptionsValidator : IOptionsValidator
{
    /// <summary>Validates file system related options and reports any detected configuration errors.</summary>
    /// <param name="options">The application options that contain file paths and flags to validate.</param>
    /// <returns>A collection of validation error messages discovered for the provided options.</returns>
    public IReadOnlyList<string> Validate(AppOptions options)
    {
        if (options.ShowHelp) return [];

        var errors = new List<string>();
        ValidateInputPath(options.InputPath, errors);
        ValidateKeyPath(options.KeyPath, errors);
        ValidateOutputPath(options.OutputPath, options.InputPath, errors);
        return errors;
    }

    /// <summary>Ensures that the input file path points to an existing file and records an error otherwise.</summary>
    /// <param name="inputPath">The user provided path to the input file.</param>
    /// <param name="errors">The list that accumulates validation error messages.</param>
    private static void ValidateInputPath(string? inputPath, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
            errors.Add($"Input file not found: {inputPath}");
    }

    /// <summary>Checks that the key file path references an existing file and logs an error when missing.</summary>
    /// <param name="keyPath">The user supplied path to the substitution key file.</param>
    /// <param name="errors">The list that collects discovered validation issues.</param>
    private static void ValidateKeyPath(string? keyPath, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(keyPath) || !File.Exists(keyPath))
            errors.Add($"Key file not found: {keyPath}");
    }

    /// <summary>Validates the output path ensuring it is distinct from the input and points to a valid file location.</summary>
    /// <param name="outputPath">The file path where results should be written.</param>
    /// <param name="inputPath">The input path used to detect conflicts with the output path.</param>
    /// <param name="errors">The list of validation errors to extend when problems are found.</param>
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

    /// <summary>Converts the supplied path to an absolute path or returns null when it cannot be resolved.</summary>
    /// <param name="path">The path string to convert to its full representation.</param>
    /// <returns>The absolute path when conversion succeeds; otherwise null.</returns>
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

    /// <summary>Determines whether two paths refer to the same location using the correct platform comparison rules.</summary>
    /// <param name="a">The first normalized path.</param>
    /// <param name="b">The second normalized path.</param>
    /// <returns><see langword="true"/> when both paths represent the same location; otherwise <see langword="false"/>.</returns>
    private static bool PathsEqual(string a, string b)
    {
        var cmp = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        return cmp.Equals(a, b);
    }
}