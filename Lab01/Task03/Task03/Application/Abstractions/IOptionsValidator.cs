using Task03.Application.Models;

namespace Task03.Application.Abstractions;

public interface IOptionsValidator
{
    /// <summary>Validates the supplied application options and returns any discovered issues.</summary>
    /// <param name="options">The application options instance that should be checked.</param>
    /// <returns>A read-only list of validation error messages produced for the options.</returns>
    IReadOnlyList<string> Validate(AppOptions options);
}