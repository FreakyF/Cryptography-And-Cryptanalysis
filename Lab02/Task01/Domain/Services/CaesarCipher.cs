using Task01.Domain.Abstractions;

namespace Task01.Domain.Services;

public class CaesarCipher : ICaesarCipher
{
    public string Encrypt(string normalizedText, string alphabet, int key)
    {
        return Transform(normalizedText, alphabet, key, true);
    }

    public string Decrypt(string normalizedText, string alphabet, int key)
    {
        return Transform(normalizedText, alphabet, key, false);
    }

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

            var newIdx = encrypt ? idx + kEff : idx - kEff;

            newIdx = Mod(newIdx, n);

            dst[i] = alphabet[newIdx];
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
}