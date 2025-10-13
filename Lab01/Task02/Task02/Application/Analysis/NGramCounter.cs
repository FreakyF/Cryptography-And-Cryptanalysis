using Task02.Application.Abstractions;

namespace Task02.Application.Analysis;

public sealed class NGramCounter : INGramCounter
{
    public IReadOnlyDictionary<string, int> Count(string normalized, int n)
    {
        ArgumentNullException.ThrowIfNull(normalized);
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "n must be >= 1.");
        if (normalized.Length < n) return new Dictionary<string, int>();

        var dict = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i <= normalized.Length - n; i++)
        {
            var gram = normalized.Substring(i, n);
            dict.TryGetValue(gram, out var c);
            dict[gram] = c + 1;
        }

        return dict;
    }
}