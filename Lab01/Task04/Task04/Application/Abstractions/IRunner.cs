using Task04.Application.Models;

namespace Task04.Application.Abstractions;

public interface IRunner
{
    /// <summary>Executes the configured workflow for the supplied options and reports an exit code.</summary>
    /// <param name="options">The parsed application options controlling execution.</param>
    /// <returns>Zero for success, one for runtime failures, and two for validation failures.</returns>
    int Run(AppOptions options);
}