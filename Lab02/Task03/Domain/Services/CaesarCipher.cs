using Task03.Domain.Abstractions;

namespace Task03.Domain.Services;

public sealed class AffineCipher : IAffineCipher
{
    /// <summary>Encrypts the supplied normalized text using the affine cipher defined by the alphabet and key pair.</summary>
    /// <param name="normalizedText">The uppercase alphabetic text to encrypt.</param>
    /// <param name="alphabet">The ordered character set that determines modulo calculations.</param>
    /// <param name="a">The multiplicative key coefficient.</param>
    /// <param name="b">The additive key coefficient.</param>
    /// <returns>The ciphertext produced by the affine transformation.</returns>
    public string Encrypt(string normalizedText, string alphabet, int a, int b)
    {
        return TransformEncrypt(normalizedText, alphabet, a, b);
    }

    /// <summary>Decrypts affine-encoded text using the provided alphabet and key pair.</summary>
    /// <param name="normalizedText">The ciphertext composed of characters from the alphabet.</param>
    /// <param name="alphabet">The ordered character set that determines modulo calculations.</param>
    /// <param name="a">The multiplicative key coefficient.</param>
    /// <param name="b">The additive key coefficient.</param>
    /// <returns>The plaintext recovered from the affine cipher.</returns>
    public string Decrypt(string normalizedText, string alphabet, int a, int b)
    {
        return TransformDecrypt(normalizedText, alphabet, a, b);
    }

    /// <summary>Applies the affine encryption formula to each character of the normalized input.</summary>
    /// <param name="text">The normalized text to encrypt.</param>
    /// <param name="alphabet">The ordered character set that provides indices for the transformation.</param>
    /// <param name="a">The multiplicative key coefficient.</param>
    /// <param name="b">The additive key coefficient.</param>
    /// <returns>The ciphertext derived from the affine transformation.</returns>
    private static string TransformEncrypt(string text, string alphabet, int a, int b)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var m = alphabet.Length;
        if (m == 0)
        {
            return string.Empty;
        }

        var map = BuildIndexMap(alphabet);

        var src = text.AsSpan();
        var dst = new char[src.Length];

        for (var i = 0; i < src.Length; i++)
        {
            var c = src[i];
            if (!map.TryGetValue(c, out var x))
            {
                throw new InvalidOperationException("Character not found in alphabet");
            }

            var encIndex = Mod(a * x + b, m);
            dst[i] = alphabet[encIndex];
        }

        return new string(dst);
    }

    /// <summary>Applies the affine decryption formula to each character of the normalized input.</summary>
    /// <param name="text">The normalized ciphertext to decrypt.</param>
    /// <param name="alphabet">The ordered character set that provides indices for the transformation.</param>
    /// <param name="a">The multiplicative key coefficient.</param>
    /// <param name="b">The additive key coefficient.</param>
    /// <returns>The plaintext reconstructed from the affine transformation.</returns>
    private static string TransformDecrypt(string text, string alphabet, int a, int b)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var m = alphabet.Length;
        if (m == 0)
        {
            return string.Empty;
        }

        var map = BuildIndexMap(alphabet);

        var aInv = ModInverse(a, m);

        var src = text.AsSpan();
        var dst = new char[src.Length];

        for (var i = 0; i < src.Length; i++)
        {
            var c = src[i];
            if (!map.TryGetValue(c, out var y))
            {
                throw new InvalidOperationException("Character not found in alphabet");
            }

            var decIndex = Mod(aInv * (y - b), m);
            dst[i] = alphabet[decIndex];
        }

        return new string(dst);
    }

    /// <summary>Creates a lookup table that maps alphabet characters to their zero-based indices.</summary>
    /// <param name="alphabet">The ordered character set used by the cipher.</param>
    /// <returns>A dictionary translating each alphabet character to its index.</returns>
    private static Dictionary<char, int> BuildIndexMap(string alphabet)
    {
        var dict = new Dictionary<char, int>(alphabet.Length);
        for (var i = 0; i < alphabet.Length; i++)
        {
            dict[alphabet[i]] = i;
        }

        return dict;
    }

    /// <summary>Computes a positive modulo result for the given value and modulus.</summary>
    /// <param name="value">The integer value to reduce.</param>
    /// <param name="m">The modulus that defines the wrap-around interval.</param>
    /// <returns>The non-negative remainder of the division.</returns>
    private static int Mod(int value, int m)
    {
        return value % m is var r && r < 0 ? r + m : r;
    }

    /// <summary>Finds the multiplicative inverse of the given value modulo the alphabet length.</summary>
    /// <param name="a">The multiplicative key coefficient to invert.</param>
    /// <param name="m">The modulus representing the alphabet length.</param>
    /// <returns>The value that satisfies a * x ≡ 1 (mod m).</returns>
    /// <exception cref="InvalidOperationException">Thrown when the coefficient has no modular inverse.</exception>
    private static int ModInverse(int a, int m)
    {
        a = Mod(a, m);
        for (var x = 1; x < m; x++)
        {
            if (Mod(a * x, m) == 1)
            {
                return x;
            }
        }

        throw new InvalidOperationException("Key 'a' is not invertible modulo alphabet length");
    }
}