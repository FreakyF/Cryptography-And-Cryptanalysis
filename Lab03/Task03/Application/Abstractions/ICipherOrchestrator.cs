using Task03.Application.Models;

namespace Task03.Application.Abstractions;

public interface ICipherOrchestrator
{
    ProcessingResult Run(Arguments args);
}