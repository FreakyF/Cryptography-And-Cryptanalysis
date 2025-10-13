using Task01.Application.Abstractions;
using Task01.Application.Models;
using Task01.Domain.Enums;

namespace Task01.Application.Validation;

public sealed class AppOptionsValidator : IOptionsValidator
{
    public IReadOnlyList<string> Validate(AppOptions options)
    {
        var errors = new List<string>();

        if (options.Mode is OperationMode.Unspecified)
            errors.Add("Mode not set. Use -e for encrypt or -d for decrypt.");

        if (string.IsNullOrWhiteSpace(options.InputPath))
            errors.Add("Missing input path. Use -i <file>.");

        if (string.IsNullOrWhiteSpace(options.OutputPath))
            errors.Add("Missing output path. Use -o <file>.");

        if (string.IsNullOrWhiteSpace(options.KeyPath))
            errors.Add("Missing key path. Use -k <file>.");

        return errors;
    }
}