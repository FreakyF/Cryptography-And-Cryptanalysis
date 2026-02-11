using Task04.Application.Abstractions;
using Task04.Application.Models;
using Task04.Domain.Enums;

namespace Task04.Application.Validation;

public sealed class AppOptionsValidator : IOptionsValidator
{
    /// <summary>Evaluates command line options ensuring exactly one mode is selected and required arguments are present.</summary>
    /// <param name="options">The parsed application options describing inputs, outputs, and requested operations.</param>
    /// <returns>A read-only list containing messages for each detected validation error.</returns>
    public IReadOnlyList<string> Validate(AppOptions options)
    {
        if (options.ShowHelp) return [];
        var errors = new List<string>();

        var hasCipher = options.Mode is not OperationMode.Unspecified;
        var hasNgrams = options.AnyNGramRequested;
        var hasChi2 = options.ComputeChiSquare || options.ReferenceOrder is not null;
        var hasBuild = options.AnyRefBuildRequested;

        ValidateModeSelection(hasCipher, hasNgrams, hasChi2, hasBuild, errors);
        if (errors.Count > 0) return errors;

        if (hasCipher) ValidateCipher(options, errors);
        if (hasNgrams) ValidateRequiresInput(options, errors);
        if (hasBuild) ValidateRequiresCorpus(options, errors);
        if (hasChi2) ValidateChiSquare(options, errors);

        return errors;
    }

    /// <summary>Verifies that exactly one high-level mode (cipher, n-gram, chi-square, build) was chosen.</summary>
    /// <param name="hasCipher">Indicates whether cipher mode was requested.</param>
    /// <param name="hasNgrams">Indicates whether n-gram generation was requested.</param>
    /// <param name="hasChi2">Indicates whether chi-square analysis was requested.</param>
    /// <param name="hasBuild">Indicates whether reference building was requested.</param>
    /// <param name="errors">The collection receiving validation error messages.</param>
    private static void ValidateModeSelection(bool hasCipher, bool hasNgrams, bool hasChi2, bool hasBuild,
        List<string> errors)
    {
        var selected = (hasCipher ? 1 : 0) + (hasNgrams ? 1 : 0) + (hasChi2 ? 1 : 0) + (hasBuild ? 1 : 0);
        switch (selected)
        {
            case 0:
                errors.Add("Select one mode: cipher | n-grams | chi-square | build-ref.");
                return;
            case > 1:
                errors.Add("Cannot mix modes. Choose exactly one.");
                break;
        }
    }

    /// <summary>Ensures cipher mode supplies the required input, output, and key paths.</summary>
    /// <param name="o">The options under validation.</param>
    /// <param name="errors">The collection receiving error messages.</param>
    private static void ValidateCipher(AppOptions o, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(o.InputPath)) errors.Add("Missing input path. Use -i <file>.");
        if (string.IsNullOrWhiteSpace(o.OutputPath)) errors.Add("Missing output path. Use -o <file>.");
        if (string.IsNullOrWhiteSpace(o.KeyPath)) errors.Add("Missing key path. Use -k <file>.");
    }

    /// <summary>Requires that operations needing only input text provide an input path.</summary>
    /// <param name="o">The options under validation.</param>
    /// <param name="errors">The collection receiving error messages.</param>
    private static void ValidateRequiresInput(AppOptions o, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(o.InputPath)) errors.Add("Missing input path. Use -i <file>.");
    }

    /// <summary>Ensures reference building operations specify the corpus input path.</summary>
    /// <param name="o">The options under validation.</param>
    /// <param name="errors">The collection receiving error messages.</param>
    private static void ValidateRequiresCorpus(AppOptions o, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(o.InputPath)) errors.Add("Missing input path. Use -i <corpus_file>.");
    }

    /// <summary>Validates chi-square mode flags, input path, reference selection, and numeric thresholds.</summary>
    /// <param name="o">The options under validation.</param>
    /// <param name="errors">The collection receiving error messages.</param>
    private static void ValidateChiSquare(AppOptions o, List<string> errors)
    {
        if (!o.ComputeChiSquare) errors.Add("Chi-square requires -s flag.");
        if (string.IsNullOrWhiteSpace(o.InputPath)) errors.Add("Missing input path. Use -i <file>.");

        var rCount = CountProvided(o.R1Path, o.R2Path, o.R3Path, o.R4Path);
        switch (rCount)
        {
            case 0:
                errors.Add("Missing reference file. Use -r1|-r2|-r3|-r4 <file>.");
                break;
            case > 1:
                errors.Add("Select exactly one reference base: -r1 or -r2 or -r3 or -r4.");
                break;
        }

        if (o.SampleLength is <= 0) errors.Add("Sample length must be > 0.");
        if (o.MinExpected is <= 0) errors.Add("Min expected (--minE) must be > 0.");
    }

    /// <summary>Counts how many reference paths were supplied.</summary>
    /// <param name="paths">The reference paths to evaluate.</param>
    /// <returns>The number of non-empty reference paths.</returns>
    private static int CountProvided(params string?[] paths) =>
        paths.Count(p => !string.IsNullOrWhiteSpace(p));
}