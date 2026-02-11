using System.Globalization;
using System.Text;
using Task01.Application.Abstractions;
using Task01.Domain;

namespace Task01.Application;

public sealed class KeyLoader(IFileReader reader) : IKeyLoader
{
    private readonly IFileReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    /// <summary>Loads a substitution key from the specified path by parsing its character mappings.</summary>
    /// <param name="path">The path to the text file that defines the substitution key.</param>
    /// <returns>A substitution key created from the parsed mappings.</returns>
    public SubstitutionKey Load(string path)
    {
        var text = _reader.ReadAll(path);
        var forward = ParseForwardMap(text);
        return SubstitutionKey.FromForward(forward);
    }

    /// <summary>Parses the raw key file text into a dictionary mapping plaintext characters to ciphertext characters.</summary>
    /// <param name="raw">The raw file contents representing the key definition.</param>
    /// <returns>A dictionary describing the forward substitution mapping.</returns>
    private static Dictionary<char, char> ParseForwardMap(string raw)
    {
        var map = new Dictionary<char, char>(26);
        using var sr = new StringReader(raw);
        var lineNo = 0;

        while (sr.ReadLine() is { } line)
        {
            lineNo++;
            var trimmed = StripComment(line).Trim();
            if (trimmed.Length == 0) continue;

            var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new InvalidDataException($"Key line {lineNo}: expected exactly 2 tokens, got {parts.Length}.");

            var lhs = ParseLetter(parts[0], lineNo, position: 1);
            var rhs = ParseLetter(parts[1], lineNo, position: 2);

            if (map.ContainsKey(lhs))
                throw new InvalidDataException($"Key line {lineNo}: duplicate left letter '{lhs}'.");

            if (map.ContainsValue(rhs))
                throw new InvalidDataException($"Key line {lineNo}: duplicate right letter '{rhs}'.");

            map[lhs] = rhs;
        }

        ValidateBijection(map);
        return map;
    }

    /// <summary>Removes comments that start with '#' from a line of key text.</summary>
    /// <param name="s">The line from which comments should be stripped.</param>
    /// <returns>The line content without any trailing comment text.</returns>
    private static string StripComment(string s)
    {
        var idx = s.IndexOf('#');
        return idx >= 0 ? s[..idx] : s;
    }

    /// <summary>Parses and validates a single letter token from the key file.</summary>
    /// <param name="token">The token text expected to contain a single letter.</param>
    /// <param name="lineNo">The line number currently being processed, used for error reporting.</param>
    /// <param name="position">The token position on the line for diagnostics.</param>
    /// <returns>The uppercase Latin letter represented by the token.</returns>
    private static char ParseLetter(string token, int lineNo, int position)
    {
        if (token.Length != 1)
            throw new InvalidDataException(
                $"Key line {lineNo}: token {position} must be single letter, got \"{token}\".");

        var c = char.ToUpperInvariant(token[0]);
        return !Alphabet.IsUpperLatin(c)
            ? throw new InvalidDataException($"Key line {lineNo}: token {position} must be A–Z, got '{token}'.")
            : c;
    }

    /// <summary>Ensures the substitution mapping defines a complete bijection over the uppercase Latin alphabet.</summary>
    /// <param name="map">The mapping to validate for completeness and uniqueness.</param>
    private static void ValidateBijection(Dictionary<char, char> map)
    {
        var missingLhs = Alphabet.LatinUpper.Where(c => !map.ContainsKey(c)).ToArray();
        var rhsSet = map.Values.ToHashSet();
        var missingRhs = Alphabet.LatinUpper.Where(c => !rhsSet.Contains(c)).ToArray();

        var sb = new StringBuilder();
        if (missingLhs.Length > 0)
            sb.Append(CultureInfo.InvariantCulture, $"Missing left letters: {string.Join(",", missingLhs)}. ");
        if (missingRhs.Length > 0)
            sb.Append(CultureInfo.InvariantCulture, $"Missing right letters: {string.Join(",", missingRhs)}. ");

        if (sb.Length > 0)
            throw new InvalidDataException($"Key validation failed. {sb}");

        if (map.Count != 26)
            throw new InvalidDataException($"Key must define 26 mappings, got {map.Count}.");
    }
}