using Task04.Domain;

namespace Task04.Application.Abstractions;

public interface ISubstitutionCipher
{
    /// <summary>Encrypts normalized plaintext using the provided substitution key.</summary>
    /// <param name="normalizedPlaintext">Uppercase alphabetic text to encrypt.</param>
    /// <param name="key">The substitution key describing forward mappings.</param>
    /// <returns>The encrypted ciphertext.</returns>
    string Encrypt(string normalizedPlaintext, SubstitutionKey key);
    /// <summary>Decrypts normalized ciphertext using the provided substitution key.</summary>
    /// <param name="normalizedCiphertext">Uppercase alphabetic text to decrypt.</param>
    /// <param name="key">The substitution key describing reverse mappings.</param>
    /// <returns>The decrypted plaintext.</returns>
    string Decrypt(string normalizedCiphertext, SubstitutionKey key);
}