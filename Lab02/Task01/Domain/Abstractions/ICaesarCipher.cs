namespace Task01.Domain.Abstractions;

public interface ICaesarCipher
{
    /// <summary>Encrypts normalized text with the supplied alphabet using the Caesar cipher and key shift.</summary>
    /// <param name="normalizedText">The uppercase, alphabet-only text to encrypt.</param>
    /// <param name="alphabet">The ordered alphabet used for indexing characters.</param>
    /// <param name="key">The shift amount applied to each character.</param>
    /// <returns>The encrypted text produced by applying the shift.</returns>
    string Encrypt(string normalizedText, string alphabet, int key);
    /// <summary>Decrypts normalized text with the supplied alphabet using the Caesar cipher and key shift.</summary>
    /// <param name="normalizedText">The uppercase, alphabet-only text to decrypt.</param>
    /// <param name="alphabet">The ordered alphabet used for indexing characters.</param>
    /// <param name="key">The shift amount applied to each character.</param>
    /// <returns>The decrypted text produced by reversing the shift.</returns>
    string Decrypt(string normalizedText, string alphabet, int key);
}