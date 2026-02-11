using Task01.Application.Models;
using Task01.Application.Services;

namespace Task01.Application.Abstractions;

public interface ICipherOrchestrator
{
    /// <summary>Coordinates reading inputs, processing text, and writing outputs for the substitution cipher operation.</summary>
    /// <param name="args">The validated arguments describing the desired cipher workflow.</param>
    /// <returns>A task that resolves to the overall processing result with exit information.</returns>
    Task<ProcessingResult> RunAsync(Arguments args);
}