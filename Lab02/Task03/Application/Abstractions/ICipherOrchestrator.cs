using Task03.Application.Models;
using Task03.Application.Services;

namespace Task03.Application.Abstractions;

public interface ICipherOrchestrator
{
    Task<ProcessingResult> RunAsync(Arguments args);
}