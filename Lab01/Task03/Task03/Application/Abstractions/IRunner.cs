using Task02.Application.Models;

namespace Task02.Application.Abstractions;

public interface IRunner
{
    int Run(AppOptions options);
}