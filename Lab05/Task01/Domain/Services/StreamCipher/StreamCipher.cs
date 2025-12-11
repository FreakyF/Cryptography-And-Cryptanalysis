using System.Runtime.CompilerServices;
using Task01.Domain.Services.Lfsr;
using Task01.Domain.Utils;

namespace Task01.Domain.Services.StreamCipher;

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

        var plainBits = BitConversions.StringToBits(plaintext);
        var count = plainBits.Count;

        if (count == 0)
        {
            return Array.Empty<bool>();
        }

        var cipherBits = GC.AllocateUninitializedArray<bool>(count);

        for (var i = 0; i < count; i++)
        {
            cipherBits[i] = plainBits[i] ^ lfsr.NextBit();
        }

        return cipherBits;
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