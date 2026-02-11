using Task02.Application.Models;
using Task02.Application.Services;

namespace Task02.Application.Abstractions;

public interface ICipherOrchestrator
{
    /// <summary>Executes the Caesar cipher workflow, including brute force, by coordinating I/O, normalization, and cipher services.</summary>
    /// <param name="args">The prepared arguments describing inputs, outputs, keys, and the requested operation.</param>
    /// <returns>A processing result indicating success information or the encountered error.</returns>
    Task<ProcessingResult> RunAsync(Arguments args);
}