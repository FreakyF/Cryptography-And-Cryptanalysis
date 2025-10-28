using Task03.Application.Models;
using Task03.Application.Services;

namespace Task03.Application.Abstractions;

public interface ICipherOrchestrator
{
    /// <summary>Executes the requested affine cipher workflow using the provided arguments.</summary>
    /// <param name="args">The parsed command-line arguments describing operation mode and file locations.</param>
    /// <returns>The result describing success or failure of the processing pipeline.</returns>
    Task<ProcessingResult> RunAsync(Arguments args);
}