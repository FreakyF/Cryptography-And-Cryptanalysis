using Task03.Application.Abstractions;

namespace Task03.Application.Text;

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