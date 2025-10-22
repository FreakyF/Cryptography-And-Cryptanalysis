using Task03.Domain;

namespace Task03.Application.Abstractions;

public interface ISubstitutionCipher
{
    /// <summary>Transforms normalized plaintext into ciphertext using the provided substitution key.</summary>
    /// <param name="normalizedPlaintext">The uppercase alphabetic plaintext prepared for encryption.</param>
    /// <param name="key">The substitution key that defines the forward character mapping.</param>
    /// <returns>The ciphertext produced by applying the substitution key to the plaintext.</returns>
    string Encrypt(string normalizedPlaintext, SubstitutionKey key);
    /// <summary>Transforms normalized ciphertext into plaintext using the provided substitution key.</summary>
    /// <param name="normalizedCiphertext">The uppercase alphabetic ciphertext prepared for decryption.</param>
    /// <param name="key">The substitution key that defines the reverse character mapping.</param>
    /// <returns>The plaintext obtained by reversing the substitution mapping.</returns>
    string Decrypt(string normalizedCiphertext, SubstitutionKey key);
}