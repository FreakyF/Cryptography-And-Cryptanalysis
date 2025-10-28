using Task04.Application.Models;
using Task04.Application.Services;

namespace Task04.Application.Abstractions;

public interface ICipherOrchestrator
{
    Task<ProcessingResult> RunAsync(Arguments args);
}