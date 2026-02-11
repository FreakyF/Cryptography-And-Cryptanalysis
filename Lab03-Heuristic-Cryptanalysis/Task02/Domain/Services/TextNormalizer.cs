using System.Runtime.CompilerServices;
using Task02.Domain.Abstractions;

namespace Task02.Domain.Services;

public sealed class TextNormalizer : ITextNormalizer
{
    /// <summary>Normalizes raw text by filtering to ASCII letters, uppercasing them, and returning the cleaned string.</summary>
    /// <param name="input">The raw user text potentially containing whitespace, punctuation, or lowercase letters.</param>
    /// <returns>The uppercase alphabet-only string, or an empty string if no letters remain.</returns>
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

    /// <summary>Determines whether the supplied character is an ASCII alphabetic letter.</summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><c>true</c> if the character is in the ranges 'A'-'Z' or 'a'-'z'; otherwise <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAsciiLetter(char c)
    {
        var v = (uint)((c | 0x20) - 'a');
        return v <= 'z' - 'a';
    }

    /// <summary>Checks whether the character is a lowercase ASCII letter for fast casing operations.</summary>
    /// <param name="c">The character to check.</param>
    /// <returns><c>true</c> if the character is 'a'-'z'; otherwise <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsLowerAscii(char c)
    {
        return (uint)(c - 'a') <= 'z' - 'a';
    }
}