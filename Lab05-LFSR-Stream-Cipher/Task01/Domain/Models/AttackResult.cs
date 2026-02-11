namespace Task01.Domain.Models;

/// <summary>
/// Represents the result of a known-plaintext attack on a stream cipher.
/// </summary>
/// <param name="FeedbackCoefficients">The recovered feedback coefficients of the LFSR.</param>
/// <param name="InitialState">The recovered initial state of the LFSR.</param>
/// <param name="KeyStream">The recovered key stream used for encryption.</param>
public sealed record AttackResult(
    IReadOnlyList<bool> FeedbackCoefficients,
    IReadOnlyList<bool> InitialState,
    IReadOnlyList<bool> KeyStream);
