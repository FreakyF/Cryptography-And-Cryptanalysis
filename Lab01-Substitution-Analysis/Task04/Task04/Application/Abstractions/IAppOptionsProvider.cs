using Task04.Application.Models;

namespace Task04.Application.Abstractions;

public interface IAppOptionsProvider
{
    /// <summary>Attempts to parse the supplied arguments into application options while reporting any errors.</summary>
    /// <param name="args">The raw command line arguments received by the application.</param>
    /// <param name="options">When successful, receives the constructed application options.</param>
    /// <param name="errors">Receives descriptive error messages explaining why parsing failed.</param>
    /// <returns><see langword="true"/> if options were produced without errors; otherwise <see langword="false"/>.</returns>
    bool TryGetOptions(string[] args, out AppOptions options, out List<string> errors);
}