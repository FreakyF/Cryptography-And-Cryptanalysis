using Task01.Application.Models;

namespace Task01.Application.Abstractions;

public interface IRunner
{
    int Run(AppOptions options);
}