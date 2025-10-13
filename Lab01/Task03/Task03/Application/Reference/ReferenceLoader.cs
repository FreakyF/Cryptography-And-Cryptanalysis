using System.Globalization;
using Task02.Application.Abstractions;

namespace Task02.Application.Reference;

public sealed class ReferenceLoader(IFileReader reader) : IReferenceLoader
{
    private readonly IFileReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    public NGramReference Load(string path)
    {
        var text = _reader.ReadAll(path);
        var dict = new Dictionary<string, double>(StringComparer.Ordinal);
        int? order = null;
        int lineNo = 0;

        using var sr = new StringReader(text);
        string? line;
        while ((line = sr.ReadLine()) is not null)
        {
            lineNo++;
            var trimmed = StripComment(line).Trim();
            if (trimmed.Length == 0) continue;

            var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new InvalidDataException($"Ref line {lineNo}: expected 2 tokens, got {parts.Length}.");

            var gram = parts[0].ToUpperInvariant();
            if (!gram.All(c => c is >= 'A' and <= 'Z'))
                throw new InvalidDataException($"Ref line {lineNo}: gram must be A–Z only, got '{parts[0]}'.");

            order ??= gram.Length;
            if (gram.Length != order.Value)
                throw new InvalidDataException($"Ref line {lineNo}: mixed n-gram lengths.");

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var p))
                throw new InvalidDataException($"Ref line {lineNo}: invalid probability '{parts[1]}'.");

            if (p < 0d || p > 1d)
                throw new InvalidDataException($"Ref line {lineNo}: probability out of range [0,1].");

            if (dict.ContainsKey(gram))
                throw new InvalidDataException($"Ref line {lineNo}: duplicate gram '{gram}'.");

            dict[gram] = p;
        }

        if (order is null || dict.Count == 0)
            throw new InvalidDataException("Reference file is empty.");

        var sum = dict.Values.Sum();
        if (Math.Abs(sum - 1d) > 1e-6)
            throw new InvalidDataException(
                $"Reference probabilities must sum to 1. Current sum={sum.ToString(CultureInfo.InvariantCulture)}.");

        return new NGramReference(order.Value, dict);
    }

    private static string StripComment(string s)
    {
        var idx = s.IndexOf('#');
        return idx >= 0 ? s[..idx] : s;
    }
}