using System.Globalization;
using Task04.Application.Abstractions;

namespace Task04.Application.Reference;

public sealed class ReferenceLoader(IFileReader reader) : IReferenceLoader
{
    private readonly IFileReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    public NGramReference Load(string path)
    {
        var text = _reader.ReadAll(path);
        var dict = new Dictionary<string, double>(StringComparer.Ordinal);
        int? order = null;
        var lineNo = 0;

        using var sr = new StringReader(text);
        while (sr.ReadLine() is { } line)
        {
            lineNo++;
            if (!TryGetTokens(line, out var tokens)) continue;

            var gram = ParseGram(tokens[0], lineNo);
            order = EnsureAndGetOrder(order, gram.Length, lineNo);

            var p = ParseProbability(tokens[1], lineNo);
            AddUnique(dict, gram, p, lineNo);
        }

        EnsureNotEmpty(order, dict);
        EnsureSumToOne(dict);

        return new NGramReference(order!.Value, dict);
    }

    private static bool TryGetTokens(string line, out string[] tokens)
    {
        var trimmed = StripComment(line).Trim();
        if (trimmed.Length == 0)
        {
            tokens = [];
            return false;
        }

        tokens = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length != 2
            ? throw new InvalidDataException($"Ref line invalid: expected 2 tokens, got {tokens.Length}.")
            : true;
    }

    private static string ParseGram(string token, int lineNo)
    {
        var gram = token.ToUpperInvariant();
        return !gram.All(c => c is >= 'A' and <= 'Z')
            ? throw new InvalidDataException($"Ref line {lineNo}: gram must be A–Z only, got '{token}'.")
            : gram;
    }

    private static int EnsureAndGetOrder(int? current, int length, int lineNo)
    {
        if (current is null) return length;
        return length != current.Value
            ? throw new InvalidDataException($"Ref line {lineNo}: mixed n-gram lengths.")
            : current.Value;
    }

    private static double ParseProbability(string token, int lineNo)
    {
        if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var p))
            throw new InvalidDataException($"Ref line {lineNo}: invalid probability '{token}'.");
        return p is < 0d or > 1d
            ? throw new InvalidDataException($"Ref line {lineNo}: probability out of range [0,1].")
            : p;
    }

    private static void AddUnique(Dictionary<string, double> dict, string gram, double p, int lineNo)
    {
        if (!dict.TryAdd(gram, p))
            throw new InvalidDataException($"Ref line {lineNo}: duplicate gram '{gram}'.");
    }

    private static void EnsureNotEmpty(int? order, Dictionary<string, double> dict)
    {
        if (order is null || dict.Count == 0)
            throw new InvalidDataException("Reference file is empty.");
    }

    private static void EnsureSumToOne(Dictionary<string, double> dict)
    {
        var sum = dict.Values.Sum();
        if (Math.Abs(sum - 1d) > 1e-6)
            throw new InvalidDataException(
                $"Reference probabilities must sum to 1. Current sum={sum.ToString(CultureInfo.InvariantCulture)}.");
    }

    private static string StripComment(string s)
    {
        var idx = s.IndexOf('#');
        return idx >= 0 ? s[..idx] : s;
    }
}