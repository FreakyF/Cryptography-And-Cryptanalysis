using Task04.Application.Models;

namespace Task04.Application.Abstractions;

public interface IRunner
{
    int Run(AppOptions options);
}