using System.Text;
using Task01.Application.Abstractions;

namespace Task01.Application.Text;

public sealed class TextNormalizer : ITextNormalizer
{
    public string Normalize(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new string(
            input.Select(char.ToUpperInvariant)
                .Where(c => c is >= 'A' and <= 'Z')
                .ToArray());
    }
}