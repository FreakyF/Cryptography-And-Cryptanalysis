using Task04.Application.Abstractions;

namespace Task04.Application.Text;

public sealed class TextNormalizer : ITextNormalizer
{
    /// <summary>Produces an uppercase alphabetic string by filtering and transforming the provided input.</summary>
    /// <param name="input">The original text that requires normalization before encryption or analysis.</param>
    /// <returns>The normalized text containing only uppercase Latin letters.</returns>
    public string Normalize(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new string(
            input.Select(char.ToUpperInvariant)
                .Where(c => c is >= 'A' and <= 'Z')
                .ToArray());
    }
}