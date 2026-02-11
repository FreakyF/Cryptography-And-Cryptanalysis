using Task01.Application.Abstractions;

namespace Task01.Application.Text;

public sealed class TextNormalizer : ITextNormalizer
{
    /// <summary>Normalizes the input by uppercasing characters and removing non-Latin letters.</summary>
    /// <param name="input">The raw text that needs to be normalized.</param>
    /// <returns>The normalized string containing only uppercase letters A through Z.</returns>
    public string Normalize(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new string(
            input.Select(char.ToUpperInvariant)
                .Where(c => c is >= 'A' and <= 'Z')
                .ToArray());
    }
}