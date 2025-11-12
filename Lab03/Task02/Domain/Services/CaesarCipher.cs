using Task02.Domain.Abstractions;

namespace Task02.Domain.Services;

public sealed class SubstitutionCipher : ISubstitutionCipher
{
    /// <summary>Encrypts normalized text using the supplied alphabet and substitution permutation.</summary>
    /// <param name="normalizedText">The uppercase text that only contains characters from the alphabet.</param>
    /// <param name="alphabet">The ordered set of characters representing the plaintext alphabet.</param>
    /// <param name="permutation">The substitution alphabet obtained by permuting the plaintext alphabet.</param>
    /// <returns>The encrypted text produced by substituting characters with their permutation counterparts.</returns>
    public string Encrypt(string normalizedText, string alphabet, string permutation)
    {
        return Transform(normalizedText, alphabet, permutation, true);
    }

    /// <summary>Decrypts normalized text using the supplied alphabet and substitution permutation.</summary>
    /// <param name="normalizedText">The uppercase text that only contains characters from the alphabet.</param>
    /// <param name="alphabet">The ordered set of characters representing the plaintext alphabet.</param>
    /// <param name="permutation">The substitution alphabet obtained by permuting the plaintext alphabet.</param>
    /// <returns>The decrypted text produced by reversing the substitution mapping.</returns>
    public string Decrypt(string normalizedText, string alphabet, string permutation)
    {
        return Transform(normalizedText, alphabet, permutation, false);
    }

    /// <summary>Transforms text by substituting each character according to the permutation direction.</summary>
    /// <param name="text">The normalized text to process.</param>
    /// <param name="alphabet">The ordered set of characters representing the plaintext alphabet.</param>
    /// <param name="permutation">The substitution alphabet obtained by permuting the plaintext alphabet.</param>
    /// <param name="encrypt">Indicates whether the transformation should encrypt or decrypt.</param>
    /// <returns>The resulting text after the substitution has been applied.</returns>
    private static string Transform(string text, string alphabet, string permutation, bool encrypt)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(alphabet) || string.IsNullOrEmpty(permutation))
        {
            return string.Empty;
        }

        if (alphabet.Length != permutation.Length)
        {
            throw new InvalidOperationException("Alphabet and permutation must be the same length");
        }

        var source = encrypt ? alphabet : permutation;
        var target = encrypt ? permutation : alphabet;

        var lookup = BuildLookup(source, target);

        var span = text.AsSpan();
        var result = new char[span.Length];

        for (var i = 0; i < span.Length; i++)
        {
            var c = span[i];

            if (!lookup.TryGetValue(c, out var mapped))
            {
                throw new InvalidOperationException("Character not found in substitution alphabet");
            }

            result[i] = mapped;
        }

        return new string(result);
    }

    /// <summary>Builds a lookup dictionary mapping characters from the source alphabet to the target alphabet.</summary>
    /// <param name="source">The alphabet providing the keys for the lookup.</param>
    /// <param name="target">The alphabet providing the mapped values.</param>
    /// <returns>A dictionary relating characters from the source alphabet to the target alphabet.</returns>
    private static Dictionary<char, char> BuildLookup(string source, string target)
    {
        var lookup = new Dictionary<char, char>(source.Length);

        for (var i = 0; i < source.Length; i++)
        {
            lookup[source[i]] = target[i];
        }

        return lookup;
    }
}