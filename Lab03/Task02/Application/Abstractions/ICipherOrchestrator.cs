using Task02.Application.Models;
using Task02.Application.Services;

namespace Task02.Application.Abstractions;

public interface ICipherOrchestrator
{
    ProcessingResult Run(Arguments args);
}