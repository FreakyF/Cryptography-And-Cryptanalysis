using System.Text;
using Task04.Application.Abstractions;
using Task04.Domain;

namespace Task04.Application.Cipher;

public sealed class SubstitutionCipher : ISubstitutionCipher
{
    /// <summary>Encrypts normalized plaintext by replacing each character using the key's forward mapping.</summary>
    /// <param name="normalizedPlaintext">The uppercase plaintext ready for encryption.</param>
    /// <param name="key">The substitution key whose forward map defines the cipher.</param>
    /// <returns>The ciphertext produced by applying the substitution.</returns>
    public string Encrypt(string normalizedPlaintext, SubstitutionKey key)
        => Transform(normalizedPlaintext, key.Forward);

    /// <summary>Decrypts normalized ciphertext by replacing each character using the key's reverse mapping.</summary>
    /// <param name="normalizedCiphertext">The uppercase ciphertext ready for decryption.</param>
    /// <param name="key">The substitution key whose reverse map restores the plaintext.</param>
    /// <returns>The plaintext recovered from the substitution.</returns>
    public string Decrypt(string normalizedCiphertext, SubstitutionKey key)
        => Transform(normalizedCiphertext, key.Reverse);

    /// <summary>Applies the provided character map to transform the entire input string.</summary>
    /// <param name="input">The normalized input text that should be transformed.</param>
    /// <param name="map">The dictionary mapping each input character to its replacement.</param>
    /// <returns>The transformed string produced by mapping each character.</returns>
    private static string Transform(string input, IReadOnlyDictionary<char, char> map)
    {
        ArgumentNullException.ThrowIfNull(input);
        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (!map.TryGetValue(c, out var m))
                throw new InvalidDataException($"Character '{c}' not present in key.");
            sb.Append(m);
        }

        return sb.ToString();
    }
}