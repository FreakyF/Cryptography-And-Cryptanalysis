using Task02.Application.Abstractions;

namespace Task02.Application.Analysis;

public sealed class ChiSquareCalculator(INGramCounter counter) : IChiSquareCalculator
{
    private readonly INGramCounter _counter = counter ?? throw new ArgumentNullException(nameof(counter));

    public double Compute(string normalizedText, int n, NGramReference reference)
    {
        if (reference is null) throw new ArgumentNullException(nameof(reference));
        if (reference.Order != n)
            throw new InvalidDataException($"Reference order {reference.Order} does not match requested n={n}.");

        var counts = _counter.Count(normalizedText ?? throw new ArgumentNullException(nameof(normalizedText)), n);
        var total = counts.Values.Sum();

        if (total == 0) return 0.0; // brak n-gramów w tekście

        // Wymuś pełne pokrycie: każdy n-gram z tekstu musi być w bazie.
        var missing = counts.Keys.Where(k => !reference.Probabilities.ContainsKey(k)).ToArray();
        if (missing.Length > 0)
        {
            var miss = string.Join(",", missing.Take(10));
            var more = missing.Length > 10 ? ", ..." : "";
            throw new InvalidDataException(
                $"Reference base misses {missing.Length} n-grams present in text: {miss}{more}");
        }

        double t = 0.0;
        foreach (var kv in reference.Probabilities)
        {
            var gram = kv.Key;
            var pi = kv.Value;
            if (pi <= 0) continue; // E_i = 0 => pomijamy wkład

            counts.TryGetValue(gram, out var ci);
            var ei = total * pi;
            var diff = ci - ei;
            t += (diff * diff) / ei;
        }

        return t;
    }
}