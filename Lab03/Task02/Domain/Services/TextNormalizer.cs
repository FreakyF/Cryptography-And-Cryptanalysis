using System.Runtime.CompilerServices;
using Task02.Domain.Abstractions;

namespace Task02.Domain.Services;

public sealed class TextNormalizer : ITextNormalizer
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public string Normalize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var src = input.AsSpan();

        var hasNonLetter = false;
        var hasLower = false;

        foreach (var c in src)
        {
            if (!IsAsciiLetter(c))
            {
                hasNonLetter = true;
            }
            else if (IsLowerAscii(c))
            {
                hasLower = true;
            }
        }

        switch (hasNonLetter)
        {
            case false when !hasLower:
                return input;
            case false:
                return string.Create(src.Length, src, static (dst, s) =>
                {
                    for (var i = 0; i < s.Length; i++)
                    {
                        var c = s[i];
                        dst[i] = IsLowerAscii(c) ? (char)(c & ~0x20) : c;
                    }
                });
        }

        var count = 0;
        foreach (var t in src)
            if (IsAsciiLetter(t)) count++;

        if (count == 0)
            return string.Empty;

        return string.Create(count, src, static (dst, s) =>
        {
            var w = 0;
            foreach (var c in s)
            {
                if (!IsAsciiLetter(c)) continue;
                dst[w++] = IsLowerAscii(c) ? (char)(c & ~0x20) : c;
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAsciiLetter(char c)
    {
        var v = (uint)((c | 0x20) - 'a');
        return v <= 'z' - 'a';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsLowerAscii(char c) => (uint)(c - 'a') <= 'z' - 'a';
}
