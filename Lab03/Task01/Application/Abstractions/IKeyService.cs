namespace Task01.Application.Abstractions;

public interface IKeyService
{
    /// <summary>Creates a random substitution permutation covering the provided alphabet.</summary>
    /// <param name="alphabet">The alphabet that the permutation must rearrange.</param>
    /// <returns>The generated permutation string.</returns>
    string CreatePermutation(string alphabet);

    /// <summary>
    ///     Extracts and validates a permutation stored alongside cipher text while returning the remaining encrypted
    ///     payload.
    /// </summary>
    /// <param name="rawInput">The raw cipher text file contents containing the persisted permutation header.</param>
    /// <param name="alphabet">The alphabet that the permutation must cover.</param>
    /// <param name="cipherText">When this method returns, contains the cipher text without the permutation header.</param>
    /// <returns>The validated permutation string ready for decryption.</returns>
    string ExtractPermutation(string rawInput, string alphabet, out string cipherText);
}