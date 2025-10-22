using Task04.Application.Abstractions;

namespace Task04.Application.Analysis;

public sealed class NGramCounter : INGramCounter
{
    /// <summary>Computes frequency counts for each n-gram within the supplied normalized text.</summary>
    /// <param name="normalized">The uppercase text to scan for n-gram occurrences.</param>
    /// <param name="n">The length of the n-grams that should be evaluated.</param>
    /// <returns>A dictionary mapping each observed n-gram to the number of times it appears.</returns>
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