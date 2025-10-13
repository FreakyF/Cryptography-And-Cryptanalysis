using Task04.Application.Models;

namespace Task04.Application.Abstractions;

public interface IOptionsValidator
{
    IReadOnlyList<string> Validate(AppOptions options);
}