using Task02.Application.Models;

namespace Task02.Application.Abstractions;

public interface IOptionsValidator
{
    IReadOnlyList<string> Validate(AppOptions options);
}