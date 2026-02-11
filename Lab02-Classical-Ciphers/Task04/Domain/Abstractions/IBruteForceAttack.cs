using Task04.Domain.Models;

namespace Task04.Domain.Abstractions;

public interface IBruteForceAttack
{
    /// <summary>Exhaustively evaluates candidate affine keys to recover plaintext from the ciphertext.</summary>
    /// <param name="cipherText">The ciphertext that should be analyzed.</param>
    /// <returns>The brute-force result containing the best plaintext and scoring details.</returns>
    BruteForceResult BreakCipher(string cipherText);
}