using Task02.Domain.Abstractions;

namespace Task02.Domain.Services;

public sealed class TextNormalizer : ITextNormalizer
{
    /// <summary>Normalizes the supplied text by filtering non-letter characters and converting remaining characters to uppercase.</summary>
    /// <param name="input">The raw text that may include whitespace, punctuation, or lowercase letters.</param>
    /// <returns>The uppercase string composed only of alphabetic characters from the input.</returns>
    public string Normalize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var span = input.AsSpan();
        var sb = new StringBuilder(span.Length);

        foreach (var c in span)
        {
            if (c is (< 'A' or > 'Z') and (< 'a' or > 'z'))
            {
                continue;
            }

            var upper = char.ToUpperInvariant(c);
            sb.Append(upper);
        }

        return sb.ToString();
    }
}