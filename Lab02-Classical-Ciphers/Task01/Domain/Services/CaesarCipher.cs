using Task01.Domain.Abstractions;

namespace Task01.Domain.Services;

public sealed class CaesarCipher : ICaesarCipher
{
    /// <summary>Encrypts normalized text using the Caesar cipher with the provided alphabet and key.</summary>
    /// <param name="normalizedText">The uppercase text that only contains characters from the alphabet.</param>
    /// <param name="alphabet">The ordered set of characters used for lookups.</param>
    /// <param name="key">The shift amount applied to each character.</param>
    /// <returns>The encrypted text produced by shifting characters forward.</returns>
    public string Encrypt(string normalizedText, string alphabet, int key)
    {
        return Transform(normalizedText, alphabet, key, true);
    }

    /// <summary>Decrypts normalized text using the Caesar cipher with the provided alphabet and key.</summary>
    /// <param name="normalizedText">The uppercase text that only contains characters from the alphabet.</param>
    /// <param name="alphabet">The ordered set of characters used for lookups.</param>
    /// <param name="key">The shift amount applied to each character.</param>
    /// <returns>The decrypted text produced by shifting characters backward.</returns>
    public string Decrypt(string normalizedText, string alphabet, int key)
    {
        return Transform(normalizedText, alphabet, key, false);
    }

    /// <summary>Transforms text by shifting each character within the alphabet according to the key and direction.</summary>
    /// <param name="text">The normalized text to process.</param>
    /// <param name="alphabet">The ordered set of characters used for substitutions.</param>
    /// <param name="key">The shift value applied when moving through the alphabet.</param>
    /// <param name="encrypt">Indicates whether the transformation should encrypt or decrypt.</param>
    /// <returns>The resulting text after the shift has been applied.</returns>
    private static string Transform(string text, string alphabet, int key, bool encrypt)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var n = alphabet.Length;
        if (n == 0)
        {
            return string.Empty;
        }

        var map = BuildIndexMap(alphabet);

        var src = text.AsSpan();
        var dst = new char[src.Length];

        var kEff = Mod(key, n);

        for (var i = 0; i < src.Length; i++)
        {
            var c = src[i];

            if (!map.TryGetValue(c, out var idx))
            {
                throw new InvalidOperationException("Character not found in alphabet");
            }

            var newIdx = encrypt
                ? idx + kEff
                : idx - kEff;

            newIdx = Mod(newIdx, n);

            dst[i] = alphabet[newIdx];
        }

        return new string(dst);
    }

    /// <summary>Builds a lookup dictionary mapping characters in the alphabet to their respective indices.</summary>
    /// <param name="alphabet">The alphabet whose characters should be indexed.</param>
    /// <returns>A dictionary that relates alphabet characters to their position.</returns>
    private static Dictionary<char, int> BuildIndexMap(string alphabet)
    {
        var dict = new Dictionary<char, int>(alphabet.Length);
        for (var i = 0; i < alphabet.Length; i++)
        {
            dict[alphabet[i]] = i;
        }

        return dict;
    }

    /// <summary>Calculates a positive modulus result for the given value and modulus base.</summary>
    /// <param name="value">The value to reduce within the modulus range.</param>
    /// <param name="m">The modulus base defining the range size.</param>
    /// <returns>The equivalent value constrained to the range [0, m).</returns>
    private static int Mod(int value, int m)
    {
        return value % m is var r && r < 0 ? r + m : r;
    }
}