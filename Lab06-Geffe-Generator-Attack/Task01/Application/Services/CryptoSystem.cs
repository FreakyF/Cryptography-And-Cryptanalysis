using System.IO;
using Lab06.Domain.Generators;
using Lab06.Infrastructure.Utils;

namespace Lab06.Application.Services;

/// <summary>
/// Provides high-level encryption and decryption services using a stream generator.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CryptoSystem"/> class.
/// </remarks>
/// <param name="generator">The stream generator used for keystream generation.</param>
public class CryptoSystem(IStreamGenerator generator)
{
    /// <summary>
    /// Encrypts a plaintext string into a sequence of bits.
    /// </summary>
    /// <param name="plainText">The plaintext string to encrypt.</param>
    /// <returns>An array of integers representing the encrypted bits.</returns>
    public int[] Encrypt(string plainText)
    {
        var messageBits = BitUtils.StringToBits(plainText);
        return ProcessBits(messageBits);
    }

    /// <summary>
    /// Decrypts a sequence of cipher bits back into a plaintext string.
    /// </summary>
    /// <param name="cipherBits">The encrypted bits to decrypt.</param>
    /// <returns>The decrypted plaintext string.</returns>
    public string Decrypt(int[] cipherBits)
    {
        var plainBits = ProcessBits(cipherBits);
        return BitUtils.BitsToString(plainBits);
    }

    /// <summary>
    /// Encrypts a file and writes the result to an output file.
    /// </summary>
    /// <param name="inputPath">The path to the source file.</param>
    /// <param name="outputPath">The path to the destination encrypted file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the input file does not exist.</exception>
    public void EncryptFile(string inputPath, string outputPath)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException($"Input file not found: {inputPath}");
        }

        var inputBytes = File.ReadAllBytes(inputPath);
        var inputBits = BitUtils.BytesToBits(inputBytes);

        var cipherBits = ProcessBits(inputBits);

        var cipherBytes = BitUtils.BitsToBytes(cipherBits);
        File.WriteAllBytes(outputPath, cipherBytes);
    }

    /// <summary>
    /// Decrypts a file and writes the result to an output file.
    /// </summary>
    /// <remarks>
    /// Since the stream cipher operation is symmetric (XOR), this method delegates to <see cref="EncryptFile"/>.
    /// </remarks>
    /// <param name="inputPath">The path to the encrypted file.</param>
    /// <param name="outputPath">The path to the destination decrypted file.</param>
    public void DecryptFile(string inputPath, string outputPath)
    {
        EncryptFile(inputPath, outputPath);
    }

    /// <summary>
    /// Processes the input bits by XORing them with the generated keystream.
    /// </summary>
    /// <param name="inputBits">The input sequence of bits.</param>
    /// <returns>The processed sequence of bits.</returns>
    private int[] ProcessBits(int[] inputBits)
    {
        var outputBits = new int[inputBits.Length];
        for (var i = 0; i < inputBits.Length; i++)
        {
            var keyBit = generator.NextBit();
            outputBits[i] = inputBits[i] ^ keyBit;
        }

        return outputBits;
    }

    /// <summary>
    /// Recovers the keystream given a known plaintext and its corresponding ciphertext.
    /// </summary>
    /// <param name="knownPlaintext">The known plaintext string.</param>
    /// <param name="cipherBits">The corresponding ciphertext bits.</param>
    /// <returns>The recovered keystream as an array of bits.</returns>
    /// <exception cref="ArgumentException">Thrown when the length of the plaintext (in bits) does not match the ciphertext length.</exception>
    public static int[] RecoverKeystream(string knownPlaintext, int[] cipherBits)
    {
        var plainBits = BitUtils.StringToBits(knownPlaintext);
        if (plainBits.Length != cipherBits.Length)
        {
            throw new ArgumentException("Length mismatch");
        }

        var keystream = new int[plainBits.Length];
        for (var i = 0; i < plainBits.Length; i++)
        {
            keystream[i] = plainBits[i] ^ cipherBits[i];
        }

        return keystream;
    }
}
