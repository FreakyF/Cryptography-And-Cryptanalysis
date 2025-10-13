using Task02.Application.Abstractions;
using Task02.Application.Models;
using Task02.Domain.Enums;

namespace Task02.Application.Validation;

public sealed class AppOptionsValidator : IOptionsValidator
{
    public IReadOnlyList<string> Validate(AppOptions options)
    {
        var errors = new List<string>();
        if (options.ShowHelp) return errors;

        var hasCipher = options.Mode is not OperationMode.Unspecified;
        var hasNgrams = options.AnyNGramRequested;
        var hasChi2 = options.ComputeChiSquare || options.ReferenceOrder is not null;

        var selectedModes =
            (hasCipher ? 1 : 0) + (hasNgrams ? 1 : 0) + (hasChi2 ? 1 : 0);
        switch (selectedModes)
        {
            case 0:
                errors.Add("No operation selected. Use cipher (-e/-d), n-grams (-g1..-g4) or chi-square (-s with -rX).");
                return errors;
            case > 1:
                errors.Add("Cannot mix modes. Choose one: cipher, n-grams, or chi-square.");
                break;
        }

        if (hasCipher)
        {
            if (string.IsNullOrWhiteSpace(options.InputPath))
                errors.Add("Missing input path. Use -i <file>.");
            if (string.IsNullOrWhiteSpace(options.OutputPath))
                errors.Add("Missing output path. Use -o <file>.");
            if (string.IsNullOrWhiteSpace(options.KeyPath))
                errors.Add("Missing key path. Use -k <file>.");
        }

        if (hasNgrams && string.IsNullOrWhiteSpace(options.InputPath))
        {
            errors.Add("Missing input path. Use -i <file>.");
        }

        if (!hasChi2) return errors;
        if (!options.ComputeChiSquare)
            errors.Add("Chi-square requires -s flag.");
        if (string.IsNullOrWhiteSpace(options.InputPath))
            errors.Add("Missing input path. Use -i <file>.");
        var rCount =
            (options.R1Path is null ? 0 : 1) +
            (options.R2Path is null ? 0 : 1) +
            (options.R3Path is null ? 0 : 1) +
            (options.R4Path is null ? 0 : 1);
        switch (rCount)
        {
            case 0:
                errors.Add("Missing reference file. Use -r1|-r2|-r3|-r4 <file>.");
                break;
            case > 1:
                errors.Add("Select exactly one reference base: -r1 or -r2 or -r3 or -r4.");
                break;
        }

        return errors;
    }
}