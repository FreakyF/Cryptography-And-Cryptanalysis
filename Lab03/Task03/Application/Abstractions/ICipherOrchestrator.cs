using Task03.Application.Models;

namespace Task03.Application.Abstractions;

public interface ICipherOrchestrator
{
    /// <summary>Runs the encryption or decryption workflow based on the provided arguments.</summary>
    /// <param name="args">The parsed command arguments that describe inputs, outputs, and desired operation.</param>
    /// <returns>The result describing the exit code and optional error message.</returns>
    ProcessingResult Run(Arguments args);
}