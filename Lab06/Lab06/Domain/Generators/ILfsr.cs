namespace Lab06.Domain.Generators;

/// <summary>
/// Defines the contract for a Linear Feedback Shift Register (LFSR).
/// </summary>
public interface ILfsr
{
    /// <summary>
    /// Generates the next bit in the pseudo-random sequence.
    /// </summary>
    /// <returns>The generated bit (0 or 1).</returns>
    int NextBit();

    /// <summary>
    /// Resets the internal state of the LFSR to the specified start state.
    /// </summary>
    /// <param name="startState">An array of integers representing the new state of the register.</param>
    void Reset(int[] startState);
}
