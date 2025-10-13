using Task01.Application.Models;

namespace Task01.Application.Abstractions;

public interface IOptionsValidator
{
    IReadOnlyList<string> Validate(AppOptions options);
}