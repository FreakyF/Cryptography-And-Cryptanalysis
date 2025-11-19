using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Task02.Domain.Abstractions;

namespace Task02.Domain.Services;

public sealed class SubstitutionCipher : ISubstitutionCipher
{
    /// <summary>Encrypts normalized text by mapping characters from the alphabet to the provided permutation.</summary>
    /// <param name="normalizedText">The uppercase plaintext comprised solely of alphabet letters.</param>
    /// <param name="alphabet">The canonical ordered alphabet describing plaintext symbols.</param>
    /// <param name="permutation">The substitution alphabet representing how plaintext maps to cipher letters.</param>
    /// <returns>The cipher text produced by substituting every character.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public string Encrypt(string normalizedText, string alphabet, string permutation)
    {
        return Transform(normalizedText, alphabet, permutation, true);
    }

    /// <summary>Decrypts normalized text by applying the inverse mapping defined by the alphabet and permutation.</summary>
    /// <param name="normalizedText">The uppercase cipher text comprised solely of alphabet letters.</param>
    /// <param name="alphabet">The canonical ordered alphabet describing plaintext symbols.</param>
    /// <param name="permutation">The substitution alphabet representing how plaintext maps to cipher letters.</param>
    /// <returns>The plaintext recovered by reversing the substitution.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public string Decrypt(string normalizedText, string alphabet, string permutation)
    {
        return Transform(normalizedText, alphabet, permutation, false);
    }

    /// <summary>Transforms text by either encrypting or decrypting according to the substitution mapping.</summary>
    /// <param name="text">The normalized text to process.</param>
    /// <param name="alphabet">The reference alphabet representing the source side of the mapping.</param>
    /// <param name="permutation">The permutation representing the target side of the mapping.</param>
    /// <param name="encrypt"><c>true</c> to encrypt, or <c>false</c> to decrypt.</param>
    /// <returns>The transformed text, or an empty string when inputs are missing.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static string Transform(string text, string alphabet, string permutation, bool encrypt)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(alphabet) || string.IsNullOrEmpty(permutation))
        {
            return string.Empty;
        }

        if (alphabet.Length != permutation.Length)
        {
            throw new InvalidOperationException("Alphabet and permutation must be the same length");
        }

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
                {
                    throw new InvalidOperationException("Character not found in substitution alphabet");
                }

                var mapped = Unsafe.Add(ref baseRef, ch);
                if (mapped < 0)
                {
                    throw new InvalidOperationException("Character not found in substitution alphabet");
                }

                dst[i] = (char)mapped;
            }
        });
    }

    /// <summary>Builds a dense lookup table that converts characters from the source alphabet to the target alphabet.</summary>
    /// <param name="source">The alphabet representing the keys of the mapping.</param>
    /// <param name="target">The alphabet representing the mapped values.</param>
    /// <returns>A dense lookup table storing the substitution mapping.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static DenseLookup BuildLookup(string source, string target)
    {
        var max = 0;
        var s = source.AsSpan();
        foreach (int c in s)
        {
            if (c > max)
            {
                max = c;
            }
        }

        var table = new int[max + 1];
        Array.Fill(table, -1);

        for (var i = 0; i < s.Length; i++)
        {
            table[s[i]] = target[i];
        }

        return new DenseLookup(table, max);
    }

    private readonly struct DenseLookup(int[] table, int maxChar)
    {
        public readonly int[] Table = table;
        public readonly int MaxChar = maxChar;
    }
}