using Task04.Application.Models;

namespace Task04.Application.Abstractions;

public interface IAppOptionsProvider
{
    bool TryGetOptions(string[] args, out AppOptions options, out List<string> errors);
}