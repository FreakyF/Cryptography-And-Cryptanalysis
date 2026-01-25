using Task01.Domain.Models;

namespace Task01.Domain.Services.Attacks;

/// <summary>
/// Defines the contract for performing a known-plaintext attack on an LFSR-based stream cipher.
/// </summary>
public interface IKnownPlaintextAttacker
{
    /// <summary>
    /// Attempts to recover the LFSR configuration (feedback coefficients and initial state) using a known plaintext segment.
    /// </summary>
    /// <param name="knownPlaintext">The known plaintext string.</param>
    /// <param name="ciphertextBits">The full ciphertext bits (must encompass the known plaintext).</param>
    /// <param name="lfsrDegree">The assumed degree of the target LFSR.</param>
    /// <returns>An <see cref="AttackResult"/> if successful; otherwise, <c>null</c>.</returns>
    AttackResult? Attack(string knownPlaintext, IReadOnlyList<bool> ciphertextBits, int lfsrDegree);
}
