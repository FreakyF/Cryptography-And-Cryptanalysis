namespace Task01.Domain.Core;

/// <summary>
///     Defines the contract for the Trivium stream cipher implementation.
/// </summary>
/// <remarks>
///     Trivium is a synchronous stream cipher designed to generate up to <c>2^64</c> bits of keystream
///     from an 80-bit secret key and an 80-bit initialization vector (IV).
///     This interface supports initialization, keystream generation, and data encryption/decryption.
/// </remarks>
public interface ITriviumCipher
{
    /// <summary>
    ///     Initializes the cipher state with the provided key and initialization vector.
    /// </summary>
    /// <param name="key">The 80-bit secret key. Must be at least 10 bytes long.</param>
    /// <param name="iv">The 80-bit initialization vector. Must be at least 10 bytes long.</param>
    /// <param name="warmupRounds">
    ///     The number of full state update rounds to perform before the cipher is ready to produce output.
    ///     The standard specification requires 1152 rounds (4 x 288).
    /// </param>
    void Initialize(byte[] key, byte[] iv, int warmupRounds = 1152);

    /// <summary>
    ///     Advances the internal state by one step and generates a single bit of keystream.
    /// </summary>
    /// <returns>The generated keystream bit as a boolean value (true for 1, false for 0).</returns>
    bool GenerateBit();

    /// <summary>
    ///     Generates a sequence of keystream bytes of the specified length.
    /// </summary>
    /// <param name="length">The number of bytes to generate.</param>
    /// <returns>A byte array containing the generated keystream.</returns>
    byte[] GenerateKeystream(int length);

    /// <summary>
    ///     Encrypts the provided plaintext by XORing it with the generated keystream.
    /// </summary>
    /// <param name="plaintext">The input data to encrypt.</param>
    /// <returns>A new byte array containing the ciphertext.</returns>
    /// <remarks>
    ///     Since Trivium is a stream cipher, encryption and decryption are symmetric operations (XOR).
    ///     This method effectively performs <c>Ciphertext = Plaintext XOR Keystream</c>.
    /// </remarks>
    byte[] Encrypt(byte[] plaintext);

    /// <summary>
    ///     Decrypts the provided ciphertext by XORing it with the generated keystream.
    /// </summary>
    /// <param name="ciphertext">The input data to decrypt.</param>
    /// <returns>A new byte array containing the plaintext.</returns>
    /// <remarks>
    ///     This method is functionally identical to <see cref="Encrypt"/> due to the involutive nature of XOR operations.
    /// </remarks>
    byte[] Decrypt(byte[] ciphertext);

    /// <summary>
    ///     Retrieves statistical metrics regarding the current internal state of the cipher.
    /// </summary>
    /// <returns>
    ///     A tuple containing:
    ///     <list type="bullet">
    ///         <item><description><c>OnesCount</c>: The total number of bits set to 1 in the 288-bit state.</description></item>
    ///         <item><description><c>Balance</c>: The ratio of ones to the total state size (OnesCount / 288.0).</description></item>
    ///     </list>
    /// </returns>
    (int OnesCount, double Balance) GetStateStatistics();
}
