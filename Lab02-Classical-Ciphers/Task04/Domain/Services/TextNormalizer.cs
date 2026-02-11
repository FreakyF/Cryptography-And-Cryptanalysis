using Task04.Domain.Abstractions;

namespace Task04.Domain.Services;

public sealed class TextNormalizer : ITextNormalizer
{
    /// <summary>Converts the input text to uppercase letters while stripping non-alphabetic characters.</summary>
    /// <param name="input">The raw text to normalize before cipher operations.</param>
    /// <returns>The uppercase alphabetic string produced from the input.</returns>
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