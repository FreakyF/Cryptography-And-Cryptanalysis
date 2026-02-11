namespace Task02.Domain.Abstractions;

public interface ICaesarCipher
{
    /// <summary>Encrypts normalized text using the Caesar cipher with the provided alphabet and key.</summary>
    /// <param name="normalizedText">The uppercase text that only contains characters from the alphabet.</param>
    /// <param name="alphabet">The ordered set of characters used for lookups.</param>
    /// <param name="key">The shift amount applied to each character.</param>
    /// <returns>The encrypted text produced by shifting characters forward.</returns>
    string Encrypt(string normalizedText, string alphabet, int key);
    /// <summary>Decrypts normalized text using the Caesar cipher with the provided alphabet and key.</summary>
    /// <param name="normalizedText">The uppercase text that only contains characters from the alphabet.</param>
    /// <param name="alphabet">The ordered set of characters used for lookups.</param>
    /// <param name="key">The shift amount applied to each character.</param>
    /// <returns>The decrypted text produced by shifting characters backward.</returns>
    string Decrypt(string normalizedText, string alphabet, int key);
}