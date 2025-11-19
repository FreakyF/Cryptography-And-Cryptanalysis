using System.Runtime.CompilerServices;
using Task03.Domain.Abstractions;

namespace Task03.Domain.Services;

public sealed class TextNormalizer : ITextNormalizer
{
    /// <summary>Filters non-letter characters and uppercases ASCII letters to produce normalized text.</summary>
    /// <param name="input">The raw text that may contain whitespace, punctuation, or lowercase letters.</param>
    /// <returns>The uppercase-only string containing just ASCII letters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public string Normalize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

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
        {
            if (IsAsciiLetter(t))
            {
                count++;
            }
        }

        if (count == 0)
        {
            return string.Empty;
        }

        return string.Create(count, src, static (dst, s) =>
        {
            var w = 0;
            foreach (var c in s)
            {
                if (!IsAsciiLetter(c))
                {
                    continue;
                }

                dst[w++] = IsLowerAscii(c) ? (char)(c & ~0x20) : c;
            }
        });
    }

    /// <summary>Indicates whether the provided character is an ASCII letter.</summary>
    /// <param name="c">The character to inspect.</param>
    /// <returns><c>true</c> when the character lies between 'A' and 'Z' (case insensitive); otherwise <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAsciiLetter(char c)
    {
        var v = (uint)((c | 0x20) - 'a');
        return v <= 'z' - 'a';
    }

    /// <summary>Determines whether the provided character is a lowercase ASCII letter.</summary>
    /// <param name="c">The character to check.</param>
    /// <returns><c>true</c> when the character is between 'a' and 'z'; otherwise <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsLowerAscii(char c)
    {
        return (uint)(c - 'a') <= 'z' - 'a';
    }
}