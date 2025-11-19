using Task02.Application.Models;
using Task02.Application.Services;

namespace Task02.Application.Abstractions;

public interface ICipherOrchestrator
{
    /// <summary>Executes the requested cipher workflow by consuming parsed arguments and returning the final status.</summary>
    /// <param name="args">The validated argument set describing the operation mode, input, output, and reference files.</param>
    /// <returns>The result object capturing the exit code and optional diagnostic message.</returns>
    ProcessingResult Run(Arguments args);
}