using Task03.Application.Models;

namespace Task03.Application.Abstractions;

public interface IRunner
{
    int Run(AppOptions options);
}