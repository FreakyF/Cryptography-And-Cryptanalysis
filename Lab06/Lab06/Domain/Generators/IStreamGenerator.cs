namespace Lab06.Domain.Generators;

/// <summary>
/// Defines the contract for a stream generator used in cryptographic systems.
/// </summary>
public interface IStreamGenerator
{
    /// <summary>
    /// Generates the next bit of the keystream.
    /// </summary>
    /// <returns>The next generated bit (0 or 1).</returns>
    int NextBit();

    /// <summary>
    /// Resets the internal states of the underlying generators (X, Y, Z) with the provided initial states.
    /// </summary>
    /// <param name="stateX">The initial state for the first generator (X).</param>
    /// <param name="stateY">The initial state for the second generator (Y).</param>
    /// <param name="stateZ">The initial state for the third generator (Z).</param>
    void Reset(int[] stateX, int[] stateY, int[] stateZ);
}
