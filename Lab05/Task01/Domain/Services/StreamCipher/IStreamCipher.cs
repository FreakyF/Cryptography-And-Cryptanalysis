using Task01.Domain.Services.Lfsr;

namespace Task01.Domain.Services.StreamCipher;

/// <summary>
/// Defines the contract for a stream cipher that uses an LFSR as a pseudo-random number generator (PRNG).
/// </summary>
public interface IStreamCipher
{
    /// <summary>
    /// Encrypts the plaintext using the provided LFSR.
    /// </summary>
    /// <param name="plaintext">The text to encrypt.</param>
    /// <param name="lfsr">The LFSR instance to use for key stream generation.</param>
    /// <returns>A read-only list of encrypted bits.</returns>
    IReadOnlyList<bool> Encrypt(string plaintext, ILfsr lfsr);

    /// <summary>
    /// Decrypts the ciphertext bits using the provided LFSR.
    /// </summary>
    /// <param name="ciphertextBits">The encrypted bits.</param>
    /// <param name="lfsr">The LFSR instance initialized with the same key/state as encryption.</param>
    /// <returns>The decrypted plaintext string.</returns>
    string Decrypt(IReadOnlyList<bool> ciphertextBits, ILfsr lfsr);
}
