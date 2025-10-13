using Task04.Application.Abstractions;
using Task04.Application.Models;

namespace Task04.Application.Analysis;

public sealed class ChiSquareCalculator(INGramCounter counter) : IChiSquareCalculator
{
    private readonly INGramCounter _counter = counter ?? throw new ArgumentNullException(nameof(counter));

    public double Compute(string normalizedText, int n, NGramReference reference, ChiSquareOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(reference);
        if (reference.Order != n) throw new InvalidDataException($"Reference order {reference.Order} != n={n}.");

        var counts = _counter.Count(normalizedText ?? throw new ArgumentNullException(nameof(normalizedText)), n);
        var total = counts.Values.Sum();
        if (total == 0) return 0.0;

        var exclude = options?.Exclude ?? new HashSet<string>(StringComparer.Ordinal);
        var minE = options?.MinExpected;

        var missing = counts.Keys.Where(k => !reference.Probabilities.ContainsKey(k) && !exclude.Contains(k)).ToArray();
        if (missing.Length > 0)
            throw new InvalidDataException(
                $"Reference base misses {missing.Length} n-grams present in text: {string.Join(",", missing.Take(10))}{(missing.Length > 10 ? ", ..." : "")}");

        var t = 0.0;
        foreach (var (g, pi) in reference.Probabilities)
        {
            if (exclude.Contains(g)) continue;

            var ei = total * pi;
            if (minE is { } thr && ei < thr) continue;

            counts.TryGetValue(g, out var ci);
            var diff = ci - ei;
            t += diff * diff / ei;
        }

        return t;
    }
}