using System.Runtime.CompilerServices;
using Task04.Domain.Abstractions;

namespace Task04.Domain.Services;

public sealed class TextNormalizer : ITextNormalizer
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public string Normalize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var src = input.AsSpan();
        var (hasNonLetter, hasLower) = DetectFlags(src);

        if (!hasNonLetter)
            return hasLower ? ToUpperAscii(src) : input;

        var letterCount = CountAsciiLetters(src);
        return letterCount == 0 ? string.Empty : FilterLettersToUpper(src, letterCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (bool hasNonLetter, bool hasLower) DetectFlags(ReadOnlySpan<char> s)
    {
        bool non = false, lower = false;
        foreach (var c in s)
        {
            if (!IsAsciiLetter(c)) { non = true; continue; }
            if (IsLowerAscii(c)) lower = true;
        }
        return (non, lower);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CountAsciiLetters(ReadOnlySpan<char> s)
    {
        int count = 0;
        foreach (var c in s)
            if (IsAsciiLetter(c)) count++;
        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToUpperAscii(ReadOnlySpan<char> s)
    {
        return string.Create(s.Length, s, static (dst, src) =>
        {
            for (int i = 0; i < src.Length; i++)
            {
                var c = src[i];
                dst[i] = IsLowerAscii(c) ? (char)(c & ~0x20) : c;
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static string FilterLettersToUpper(ReadOnlySpan<char> s, int letterCount)
    {
        return string.Create(letterCount, s, static (dst, src) =>
        {
            int w = 0;
            foreach (var c in src)
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
