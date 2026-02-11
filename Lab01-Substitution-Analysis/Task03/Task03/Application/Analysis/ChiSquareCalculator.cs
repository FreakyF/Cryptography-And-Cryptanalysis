using Task03.Application.Abstractions;

namespace Task03.Application.Analysis;

public sealed class ChiSquareCalculator(INGramCounter counter) : IChiSquareCalculator
{
    private readonly INGramCounter _counter = counter ?? throw new ArgumentNullException(nameof(counter));

    /// <summary>Calculates the chi-square statistic by comparing observed n-gram counts with reference probabilities.</summary>
    /// <param name="normalizedText">The normalized text whose n-gram counts are evaluated.</param>
    /// <param name="n">The n-gram order to analyze.</param>
    /// <param name="reference">The reference distribution expected for the chosen n-gram order.</param>
    /// <returns>The chi-square statistic measuring divergence between observed and expected frequencies.</returns>
    public double Compute(string normalizedText, int n, NGramReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        if (reference.Order != n)
            throw new InvalidDataException($"Reference order {reference.Order} does not match requested n={n}.");

        var counts = _counter.Count(normalizedText ?? throw new ArgumentNullException(nameof(normalizedText)), n);
        var total = counts.Values.Sum();

        if (total == 0) return 0.0;

        var missing = counts.Keys.Where(k => !reference.Probabilities.ContainsKey(k)).ToArray();
        if (missing.Length > 0)
        {
            var miss = string.Join(",", missing.Take(10));
            var more = missing.Length > 10 ? ", ..." : "";
            throw new InvalidDataException(
                $"Reference base misses {missing.Length} n-grams present in text: {miss}{more}");
        }

        var t = 0.0;
        foreach (var (gram, pi) in reference.Probabilities)
        {
            if (pi <= 0) continue;

            counts.TryGetValue(gram, out var ci);
            var ei = total * pi;
            var diff = ci - ei;
            t += diff * diff / ei;
        }

        return t;
    }
}