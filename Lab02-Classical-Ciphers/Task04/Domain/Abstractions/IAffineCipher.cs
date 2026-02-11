namespace Task04.Domain.Abstractions;

public interface IAffineCipher
{
    /// <summary>Encrypts normalized text using the affine transformation defined by the alphabet and key pair.</summary>
    /// <param name="normalizedText">The uppercase alphabetic text to be encrypted.</param>
    /// <param name="alphabet">The sequence of allowable characters that defines modulo operations.</param>
    /// <param name="a">The multiplicative component of the affine key.</param>
    /// <param name="b">The additive component of the affine key.</param>
    /// <returns>The ciphertext produced by applying the affine cipher.</returns>
    string Encrypt(string normalizedText, string alphabet, int a, int b);
    /// <summary>Decrypts affine-encrypted text using the provided alphabet and key pair.</summary>
    /// <param name="normalizedText">The ciphertext to be decrypted, composed of characters from the alphabet.</param>
    /// <param name="alphabet">The sequence of allowable characters that defines modulo operations.</param>
    /// <param name="a">The multiplicative component of the affine key.</param>
    /// <param name="b">The additive component of the affine key.</param>
    /// <returns>The plaintext recovered from the affine cipher.</returns>
    string Decrypt(string normalizedText, string alphabet, int a, int b);
}