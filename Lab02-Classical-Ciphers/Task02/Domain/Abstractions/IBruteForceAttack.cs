using Task02.Domain.Models;

namespace Task02.Domain.Abstractions;

public interface IBruteForceAttack
{
    /// <summary>Attempts to recover the plaintext and key by evaluating every possible Caesar cipher shift.</summary>
    /// <param name="cipherText">The normalized ciphertext to analyze.</param>
    /// <returns>A result describing the best candidate plaintext, key, and score.</returns>
    BruteForceResult BreakCipher(string cipherText);
}