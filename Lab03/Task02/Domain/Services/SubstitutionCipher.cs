using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Task02.Domain.Abstractions;

namespace Task02.Domain.Services;

public sealed class SubstitutionCipher : ISubstitutionCipher
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public string Encrypt(string normalizedText, string alphabet, string permutation)
        => Transform(normalizedText, alphabet, permutation, encrypt: true);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public string Decrypt(string normalizedText, string alphabet, string permutation)
        => Transform(normalizedText, alphabet, permutation, encrypt: false);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static string Transform(string text, string alphabet, string permutation, bool encrypt)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(alphabet) || string.IsNullOrEmpty(permutation))
            return string.Empty;

        if (alphabet.Length != permutation.Length)
            throw new InvalidOperationException("Alphabet and permutation must be the same length");

        var source = encrypt ? alphabet : permutation;
        var target = encrypt ? permutation : alphabet;

        var lookup = BuildLookup(source, target);

        return string.Create(text.Length, (text, lookup), static (dst, state) =>
        {
            var (srcText, map) = state;
            var src = srcText.AsSpan();

            ref var baseRef = ref MemoryMarshal.GetArrayDataReference(map.Table);
            var max = map.MaxChar;

            for (var i = 0; i < src.Length; i++)
            {
                int ch = src[i];
                if ((uint)ch > (uint)max)
                    throw new InvalidOperationException("Character not found in substitution alphabet");

                var mapped = Unsafe.Add(ref baseRef, ch);
                if (mapped < 0)
                    throw new InvalidOperationException("Character not found in substitution alphabet");

                dst[i] = (char)mapped;
            }
        });
    }

    private readonly struct DenseLookup(int[] table, int maxChar)
    {
        public readonly int[] Table = table;
        public readonly int MaxChar = maxChar;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static DenseLookup BuildLookup(string source, string target)
    {
        var max = 0;
        var s = source.AsSpan();
        foreach (int c in s)
        {
            if (c > max) max = c;
        }

        var table = new int[max + 1];
        Array.Fill(table, -1);

        for (var i = 0; i < s.Length; i++)
        {
            table[s[i]] = target[i];
        }

        return new DenseLookup(table, max);
    }
}