using Task01.Application.Models;

namespace Task01.Application.Abstractions;

public interface IAppOptionsProvider
{
    bool TryGetOptions(string[] args, out AppOptions options, out List<string> errors);
}