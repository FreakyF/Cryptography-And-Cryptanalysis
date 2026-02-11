using Task02.Application.Abstractions;
using Task02.Application.Models;
using Task02.Domain.Enums;

namespace Task02.Application.Validation;

public sealed class AppOptionsValidator : IOptionsValidator
{
    /// <summary>Evaluates the command line options to ensure required values and mode combinations are valid.</summary>
    /// <param name="options">The parsed application options describing inputs, outputs, and requested operations.</param>
    /// <returns>A read-only list containing messages for each detected validation error.</returns>
    public IReadOnlyList<string> Validate(AppOptions options)
    {
        var errors = new List<string>();
        if (options.ShowHelp) return errors;

        var hasCipher = options.Mode is not OperationMode.Unspecified;
        var hasNgrams = options.AnyNGramRequested;

        switch (hasCipher)
        {
            case false when !hasNgrams:
                errors.Add("No operation selected. Use -e/-d for cipher or -g1..-g4 for n-grams.");
                return errors;
            case true when hasNgrams:
                errors.Add("Cannot mix cipher mode with n-gram generation.");
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
        else
        {
            if (string.IsNullOrWhiteSpace(options.InputPath))
                errors.Add("Missing input path. Use -i <file>.");
        }

        return errors;
    }
}