using Task04.Application.Models;

namespace Task04.Application.Abstractions;

public interface IOptionsValidator
{
    /// <summary>Validates the provided options and returns any discovered error messages.</summary>
    /// <param name="options">The options object to examine.</param>
    /// <returns>A collection of validation errors; empty when options are valid.</returns>
    IReadOnlyList<string> Validate(AppOptions options);
}