using Task01.Application.Models;
using Task01.Application.Services;

namespace Task01.Application.Abstractions;

public interface ICipherOrchestrator
{
    Task<ProcessingResult> RunAsync(Arguments args);
}