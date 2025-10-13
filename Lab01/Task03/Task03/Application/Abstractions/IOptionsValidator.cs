using Task03.Application.Models;

namespace Task03.Application.Abstractions;

public interface IOptionsValidator
{
    IReadOnlyList<string> Validate(AppOptions options);
}