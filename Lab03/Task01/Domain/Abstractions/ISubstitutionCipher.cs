namespace Task01.Domain.Abstractions;

public interface ISubstitutionCipher
{
    /// <summary>Encrypts normalized text using a substitution alphabet derived from the provided permutation.</summary>
    /// <param name="normalizedText">The uppercase, alphabet-only text to encrypt.</param>
    /// <param name="alphabet">The ordered alphabet representing the plaintext characters.</param>
    /// <param name="permutation">The substitution alphabet representing a permutation of the plaintext alphabet.</param>
    /// <returns>The encrypted text produced by substituting each character with the permutation counterpart.</returns>
    string Encrypt(string normalizedText, string alphabet, string permutation);

    /// <summary>Decrypts normalized text using a substitution alphabet derived from the provided permutation.</summary>
    /// <param name="normalizedText">The uppercase, alphabet-only text to decrypt.</param>
    /// <param name="alphabet">The ordered alphabet representing the plaintext characters.</param>
    /// <param name="permutation">The substitution alphabet representing a permutation of the plaintext alphabet.</param>
    /// <returns>The decrypted text produced by reversing the substitution mapping.</returns>
    string Decrypt(string normalizedText, string alphabet, string permutation);
}