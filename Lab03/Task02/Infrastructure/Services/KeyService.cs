using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Task02.Application.Abstractions;

namespace Task02.Infrastructure.Services;

public sealed class KeyService : IKeyService
{
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

        var permutation = string.Create(keySpan.Length, keySpan, static (dst, src) =>
        {
            var i = 0;
            var c = src[i];
            for (; i < src.Length; i++)
            {
                dst[i] = (char)((uint)(c - 'a') <= 25u ? c & ~0x20 : c);
            }
        });

        if (!IsValidPermutation(permutation, alphabet))
        {
            throw new FormatException("Key is not a valid permutation");
        }

        cipherText = remainder.Length == 0 ? string.Empty : remainder.ToString();
        return permutation;
    }

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

    private static ReadOnlySpan<char> TrimWhite(ReadOnlySpan<char> value)
    {
        int start = 0, end = value.Length - 1;
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


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static bool IsValidPermutation(string permutation, string alphabet)
    {
        if (permutation.Length != alphabet.Length)
        {
            return false;
        }

        var aSpan = alphabet.AsSpan();
        var max = 0;
        foreach (int c in aSpan)
        {
            if (c > max)
            {
                max = c;
            }
        }

        var expected = new sbyte[max + 1];
        var seen = new sbyte[max + 1];

        foreach (var t in aSpan)
        {
            expected[t] = 1;
        }

        var pSpan = permutation.AsSpan();
        foreach (int c in pSpan)
        {
            if ((uint)c > (uint)max)
            {
                return false;
            }

            if (expected[c] == 0)
            {
                return false;
            }

            if (seen[c] != 0)
            {
                return false;
            }

            seen[c] = 1;
        }

        return true;
    }
}