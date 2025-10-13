using Task02.Application.Models;

namespace Task02.Application.Abstractions;

public interface IAppOptionsProvider
{
    bool TryGetOptions(string[] args, out AppOptions options, out List<string> errors);
}