using Task01.Domain.Abstractions;

namespace Task01.Domain.Services;

public class TextNormalizer : ITextNormalizer
{
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
            if (!char.IsLetter(c))
            {
                continue;
            }

            var upper = char.ToUpperInvariant(c);
            sb.Append(upper);
        }

        return sb.ToString();
    }
}