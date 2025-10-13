using Task03.Application.Abstractions;
using Task03.Application.Models;
using Task03.Domain.Enums;

namespace Task03.Application.Validation;

public sealed class AppOptionsValidator : IOptionsValidator
{
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

    private static void ValidateCipher(AppOptions o, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(o.InputPath))
            errors.Add("Missing input path. Use -i <file>.");
        if (string.IsNullOrWhiteSpace(o.OutputPath))
            errors.Add("Missing output path. Use -o <file>.");
        if (string.IsNullOrWhiteSpace(o.KeyPath))
            errors.Add("Missing key path. Use -k <file>.");
    }

    private static void ValidateRequiresInput(AppOptions o, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(o.InputPath))
            errors.Add("Missing input path. Use -i <file>.");
    }

    private static void ValidateRequiresCorpus(AppOptions o, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(o.InputPath))
            errors.Add("Missing input path. Use -i <corpus_file>.");
    }

    private static void ValidateChiSquare(AppOptions o, List<string> errors)
    {
        if (!o.ComputeChiSquare)
            errors.Add("Chi-square requires -s flag.");

        if (string.IsNullOrWhiteSpace(o.InputPath))
            errors.Add("Missing input path. Use -i <file>.");

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
    }

    private static int CountProvided(params string?[] paths) =>
        paths.Count(p => !string.IsNullOrWhiteSpace(p));
}