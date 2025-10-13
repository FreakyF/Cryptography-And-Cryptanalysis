using Task03.Application.Models;

namespace Task03.Application.Abstractions;

public interface IAppOptionsProvider
{
    bool TryGetOptions(string[] args, out AppOptions options, out List<string> errors);
}