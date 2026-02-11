using System.Globalization;
using Task04.Application.Abstractions;

namespace Task04.Application.Reference;

public sealed class ReferenceLoader(IFileReader reader) : IReferenceLoader
{
    private readonly IFileReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    /// <summary>Loads an n-gram reference file and produces a validated probability distribution.</summary>
    /// <param name="path">The file path containing n-gram probabilities.</param>
    /// <returns>A reference instance describing the n-gram order and probability map.</returns>
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

    /// <summary>Extracts meaningful tokens from a line while ignoring comments and blank lines.</summary>
    /// <param name="line">The raw line read from the reference file.</param>
    /// <param name="tokens">Receives the two tokens when parsing succeeds.</param>
    /// <returns><see langword="true"/> when tokens were produced; otherwise <see langword="false"/> to skip the line.</returns>
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

    /// <summary>Validates that a token represents a valid uppercase n-gram.</summary>
    /// <param name="token">The token containing the n-gram text.</param>
    /// <param name="lineNo">The current line number for error reporting.</param>
    /// <returns>The uppercase n-gram string.</returns>
    private static string ParseGram(string token, int lineNo)
    {
        var gram = token.ToUpperInvariant();
        return !gram.All(c => c is >= 'A' and <= 'Z')
            ? throw new InvalidDataException($"Ref line {lineNo}: gram must be A–Z only, got '{token}'.")
            : gram;
    }

    /// <summary>Ensures that all parsed n-grams share the same length and returns the expected order.</summary>
    /// <param name="current">The previously recorded order, if any.</param>
    /// <param name="length">The length of the n-gram discovered on the current line.</param>
    /// <param name="lineNo">The line number used for diagnostics when a mismatch occurs.</param>
    /// <returns>The established n-gram order.</returns>
    private static int EnsureAndGetOrder(int? current, int length, int lineNo)
    {
        if (current is null) return length;
        return length != current.Value
            ? throw new InvalidDataException($"Ref line {lineNo}: mixed n-gram lengths.")
            : current.Value;
    }

    /// <summary>Parses a probability token and validates it lies within the inclusive [0,1] range.</summary>
    /// <param name="token">The textual probability value.</param>
    /// <param name="lineNo">The line number for error messages.</param>
    /// <returns>The parsed double precision probability.</returns>
    private static double ParseProbability(string token, int lineNo)
    {
        if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var p))
            throw new InvalidDataException($"Ref line {lineNo}: invalid probability '{token}'.");
        return p is < 0d or > 1d
            ? throw new InvalidDataException($"Ref line {lineNo}: probability out of range [0,1].")
            : p;
    }

    /// <summary>Adds an n-gram probability to the dictionary while preventing duplicates.</summary>
    /// <param name="dict">The accumulating probability dictionary.</param>
    /// <param name="gram">The n-gram key being inserted.</param>
    /// <param name="p">The probability value associated with the n-gram.</param>
    /// <param name="lineNo">The current line number for duplicate diagnostics.</param>
    private static void AddUnique(Dictionary<string, double> dict, string gram, double p, int lineNo)
    {
        if (!dict.TryAdd(gram, p))
            throw new InvalidDataException($"Ref line {lineNo}: duplicate gram '{gram}'.");
    }

    /// <summary>Verifies that the reference file contained at least one valid n-gram entry.</summary>
    /// <param name="order">The detected n-gram order.</param>
    /// <param name="dict">The probability dictionary constructed from the file.</param>
    private static void EnsureNotEmpty(int? order, Dictionary<string, double> dict)
    {
        if (order is null || dict.Count == 0)
            throw new InvalidDataException("Reference file is empty.");
    }

    /// <summary>Confirms that all probability values sum to one within a small tolerance.</summary>
    /// <param name="dict">The probability dictionary to validate.</param>
    private static void EnsureSumToOne(Dictionary<string, double> dict)
    {
        var sum = dict.Values.Sum();
        if (Math.Abs(sum - 1d) > 1e-6)
            throw new InvalidDataException(
                $"Reference probabilities must sum to 1. Current sum={sum.ToString(CultureInfo.InvariantCulture)}.");
    }

    /// <summary>Removes trailing comments beginning with '#' from the provided line.</summary>
    /// <param name="s">The line text that may contain a comment.</param>
    /// <returns>The line content without any comment portion.</returns>
    private static string StripComment(string s)
    {
        var idx = s.IndexOf('#');
        return idx >= 0 ? s[..idx] : s;
    }
}