using Task03.Domain.Abstractions;

namespace Task03.Domain.Services;

public sealed class AffineCipher : IAffineCipher
{
    public string Encrypt(string normalizedText, string alphabet, int a, int b)
    {
        return TransformEncrypt(normalizedText, alphabet, a, b);
    }

    public string Decrypt(string normalizedText, string alphabet, int a, int b)
    {
        return TransformDecrypt(normalizedText, alphabet, a, b);
    }

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

    private static Dictionary<char, int> BuildIndexMap(string alphabet)
    {
        var dict = new Dictionary<char, int>(alphabet.Length);
        for (var i = 0; i < alphabet.Length; i++)
        {
            dict[alphabet[i]] = i;
        }

        return dict;
    }

    private static int Mod(int value, int m)
    {
        return value % m is var r && r < 0 ? r + m : r;
    }

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