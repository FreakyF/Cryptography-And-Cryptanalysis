using System.Runtime.CompilerServices;
using Task01.Domain.Services.Lfsr;
using Task01.Domain.Utils;

namespace Task01.Domain.Services.StreamCipher;

/// <summary>
/// Provides a high-performance implementation of a stream cipher using an LFSR for keystream generation.
/// </summary>
public sealed class StreamCipher : IStreamCipher
{
    /// <summary>
    /// Encrypts the plaintext using the provided LFSR.
    /// </summary>
    /// <param name="plaintext">The text to encrypt.</param>
    /// <param name="lfsr">The LFSR instance to use for key stream generation.</param>
    /// <returns>A read-only list of encrypted bits.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plaintext"/> or <paramref name="lfsr"/> is null.</exception>
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

    /// <summary>
    /// Decrypts the ciphertext bits using the provided LFSR.
    /// </summary>
    /// <param name="ciphertextBits">The encrypted bits.</param>
    /// <param name="lfsr">The LFSR instance initialized with the same key/state as encryption.</param>
    /// <returns>The decrypted plaintext string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ciphertextBits"/> or <paramref name="lfsr"/> is null.</exception>
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
