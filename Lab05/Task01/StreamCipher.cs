using System.Runtime.CompilerServices;

namespace Task01;

public sealed class StreamCipher : IStreamCipher
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public IReadOnlyList<bool> Encrypt(string plaintext, ILfsr lfsr)
    {
        if (plaintext == null)
        {
            throw new ArgumentNullException(nameof(plaintext));
        }

        if (lfsr == null)
        {
            throw new ArgumentNullException(nameof(lfsr));
        }

        return lfsr is Lfsr concrete
            ? EncryptCoreAscii(plaintext, concrete)
            : EncryptGeneric(plaintext, lfsr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public string Decrypt(IReadOnlyList<bool> ciphertextBits, ILfsr lfsr)
    {
        if (ciphertextBits == null)
        {
            throw new ArgumentNullException(nameof(ciphertextBits));
        }

        if (lfsr == null)
        {
            throw new ArgumentNullException(nameof(lfsr));
        }

        if (ciphertextBits.Count == 0)
        {
            return string.Empty;
        }

        return ciphertextBits is bool[] array && lfsr is Lfsr concrete
            ? DecryptCoreAscii(array, concrete)
            : DecryptGeneric(ciphertextBits, lfsr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static IReadOnlyList<bool> EncryptCoreAscii(string plaintext, Lfsr lfsr)
    {
        var length = plaintext.Length;
        var bitCount = length * 8;

        if (bitCount == 0)
        {
            return Array.Empty<bool>();
        }

        var cipherBits = GC.AllocateUninitializedArray<bool>(bitCount);
        var index = 0;

        for (var i = 0; i < length; i++)
        {
            var value = (byte)plaintext[i];

            for (var bit = 7; bit >= 0; bit--)
            {
                var plainBit = ((value >> bit) & 1) != 0;
                var keyBit = lfsr.NextBit();
                cipherBits[index++] = plainBit ^ keyBit;
            }
        }

        return cipherBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static IReadOnlyList<bool> EncryptGeneric(string plaintext, ILfsr lfsr)
    {
        var plainBits = BitConversions.StringToBits(plaintext);
        var cipherBits = GC.AllocateUninitializedArray<bool>(plainBits.Count);

        for (var i = 0; i < plainBits.Count; i++)
        {
            cipherBits[i] = plainBits[i] ^ lfsr.NextBit();
        }

        return cipherBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static string DecryptCoreAscii(bool[] ciphertextBits, Lfsr lfsr)
    {
        var bitCount = ciphertextBits.Length;
        var charCount = bitCount / 8;

        if (charCount == 0)
        {
            return string.Empty;
        }

        return string.Create(
            charCount,
            (ciphertextBits, lfsr),
            static (span, state) =>
            {
                var (bits, l) = state;
                var index = 0;

                for (var i = 0; i < span.Length; i++)
                {
                    byte value = 0;

                    for (var bit = 7; bit >= 0; bit--)
                    {
                        var cipherBit = bits[index++];
                        var keyBit = l.NextBit();

                        if (cipherBit ^ keyBit)
                        {
                            value |= (byte)(1 << bit);
                        }
                    }

                    span[i] = (char)value;
                }
            });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static string DecryptGeneric(IReadOnlyList<bool> ciphertextBits, ILfsr lfsr)
    {
        var count = ciphertextBits.Count;
        if (count == 0)
        {
            return string.Empty;
        }

        var plainBits = GC.AllocateUninitializedArray<bool>(count);

        for (var i = 0; i < count; i++)
        {
            plainBits[i] = ciphertextBits[i] ^ lfsr.NextBit();
        }

        return BitConversions.BitsToString(plainBits);
    }
}
