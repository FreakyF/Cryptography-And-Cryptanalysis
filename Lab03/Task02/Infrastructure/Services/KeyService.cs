using System.Security.Cryptography;
using Task02.Application.Abstractions;

namespace Task02.Infrastructure.Services;

public sealed class KeyService : IKeyService
{
    /// <summary>Creates a random substitution permutation covering the provided alphabet.</summary>
    /// <param name="alphabet">The alphabet that the permutation must rearrange.</param>
    /// <returns>The generated permutation string.</returns>
    public string CreatePermutation(string alphabet)
    {
        if (string.IsNullOrEmpty(alphabet))
        {
            throw new FormatException("Alphabet is empty");
        }

        var chars = alphabet.ToCharArray();
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    /// <summary>
    ///     Extracts and validates a permutation stored alongside cipher text while returning the remaining encrypted
    ///     payload.
    /// </summary>
    /// <param name="rawInput">The raw cipher text file contents containing the persisted permutation header.</param>
    /// <param name="alphabet">The alphabet that the permutation must cover.</param>
    /// <param name="cipherText">When this method returns, contains the cipher text without the permutation header.</param>
    /// <returns>The validated permutation string ready for decryption.</returns>
    public string ExtractPermutation(string rawInput, string alphabet, out string cipherText)
    {
        if (string.IsNullOrEmpty(alphabet))
        {
            throw new FormatException("Alphabet is empty");
        }

        if (string.IsNullOrEmpty(rawInput))
        {
            throw new FormatException("Key not found");
        }

        var span = rawInput.AsSpan();
        var keySpan = ReadFirstLine(span, out var remainder);

        keySpan = TrimWhite(keySpan);
        if (keySpan.IsEmpty)
        {
            throw new FormatException("Key not found");
        }

        var permutation = new string(keySpan).ToUpperInvariant();

        if (!IsValidPermutation(permutation, alphabet))
        {
            throw new FormatException("Key is not a valid permutation");
        }

        cipherText = remainder.Length == 0 ? string.Empty : remainder.ToString();

        return permutation;
    }

    /// <summary>Reads the first line from a span and returns the remainder after the line ending.</summary>
    /// <param name="value">The span of characters that starts with the permutation header.</param>
    /// <param name="remainder">The span that follows the first line, excluding line endings.</param>
    /// <returns>The span containing the first line prior to the line ending.</returns>
    private static ReadOnlySpan<char> ReadFirstLine(ReadOnlySpan<char> value, out ReadOnlySpan<char> remainder)
    {
        var index = 0;
        while (index < value.Length && value[index] is not ('\r' or '\n'))
        {
            index++;
        }

        if (index == value.Length)
        {
            throw new FormatException("Cipher text not found");
        }

        var line = value[..index];
        var next = index;

        if (next < value.Length && value[next] == '\r')
        {
            next++;
            if (next < value.Length && value[next] == '\n')
            {
                next++;
            }
        }
        else if (next < value.Length && value[next] == '\n')
        {
            next++;
        }

        while (next < value.Length && value[next] is '\r' or '\n')
        {
            next++;
        }

        remainder = next < value.Length ? value[next..] : ReadOnlySpan<char>.Empty;
        return line;
    }

    /// <summary>Trims leading and trailing whitespace characters from the provided span.</summary>
    /// <param name="value">The span of characters to trim.</param>
    /// <returns>The span without surrounding whitespace, or empty if only whitespace was present.</returns>
    private static ReadOnlySpan<char> TrimWhite(ReadOnlySpan<char> value)
    {
        var start = 0;
        var end = value.Length - 1;

        while (start <= end && char.IsWhiteSpace(value[start]))
        {
            start++;
        }

        while (end >= start && char.IsWhiteSpace(value[end]))
        {
            end--;
        }

        return start > end ? ReadOnlySpan<char>.Empty : value.Slice(start, end - start + 1);
    }

    /// <summary>Determines whether the provided permutation is a valid rearrangement of the alphabet.</summary>
    /// <param name="permutation">The permutation string to validate.</param>
    /// <param name="alphabet">The alphabet that should be represented by the permutation.</param>
    /// <returns><see langword="true" /> if the permutation is valid; otherwise, <see langword="false" />.</returns>
    private static bool IsValidPermutation(string permutation, string alphabet)
    {
        if (permutation.Length != alphabet.Length)
        {
            return false;
        }

        var expected = new HashSet<char>(alphabet.Length);
        foreach (var ch in alphabet)
        {
            expected.Add(ch);
        }

        var seen = new HashSet<char>(permutation.Length);
        if (permutation.Any(ch => !expected.Contains(ch) || !seen.Add(ch)))
        {
            return false;
        }

        return seen.Count == expected.Count;
    }
}