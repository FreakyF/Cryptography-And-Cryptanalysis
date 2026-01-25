namespace Task01.Domain.Services.Lfsr;

/// <summary>
/// Defines the contract for a Linear Feedback Shift Register (LFSR).
/// </summary>
public interface ILfsr
{
    /// <summary>
    /// Gets the degree of the LFSR (length of the register).
    /// </summary>
    int Degree { get; }

    /// <summary>
    /// Gets the feedback coefficients (connection polynomial).
    /// </summary>
    IReadOnlyList<bool> FeedbackCoefficients { get; }

    /// <summary>
    /// Gets the current state of the LFSR.
    /// </summary>
    IReadOnlyList<bool> State { get; }

    /// <summary>
    /// Advances the LFSR by one step and returns the output bit.
    /// </summary>
    /// <returns>The generated bit.</returns>
    bool NextBit();

    /// <summary>
    /// Resets the LFSR to a specified initial state.
    /// </summary>
    /// <param name="state">The new state to load.</param>
    void Reset(IEnumerable<bool> state);

    /// <summary>
    /// Generates a sequence of bits by advancing the LFSR multiple times.
    /// </summary>
    /// <param name="count">The number of bits to generate.</param>
    /// <returns>A read-only list of generated bits.</returns>
    IReadOnlyList<bool> GenerateBits(int count);
}
