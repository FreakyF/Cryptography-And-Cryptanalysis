using Task04.Application.Models;
using Task04.Application.Services;

namespace Task04.Application.Abstractions;

public interface ICipherOrchestrator
{
    /// <summary>Executes the requested cipher workflow using the supplied arguments.</summary>
    /// <param name="args">The parsed operation details and file paths.</param>
    /// <returns>The result describing the exit code and optional status message.</returns>
    Task<ProcessingResult> RunAsync(Arguments args);
}