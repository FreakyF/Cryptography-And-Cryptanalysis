namespace Lab06.Domain.Generators;

/// <summary>
/// Represents a combination generator (e.g., Geffe generator) that combines outputs from multiple LFSRs.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CombinationGenerator"/> class.
/// </remarks>
/// <param name="x">The first LFSR input.</param>
/// <param name="y">The second LFSR input (often acts as the combiner).</param>
/// <param name="z">The third LFSR input.</param>
public class CombinationGenerator(ILfsr x, ILfsr y, ILfsr z) : IStreamGenerator
{
    /// <summary>
    /// Generates the next keystream bit using the combining function.
    /// </summary>
    /// <remarks>
    /// The combining function is defined as: f(x, y, z) = (x AND y) XOR (y AND z) XOR z.
    /// </remarks>
    /// <returns>The calculated output bit.</returns>
    public int NextBit()
    {
        var x1 = x.NextBit();
        var y1 = y.NextBit();
        var z1 = z.NextBit();

        return (x1 & y1) ^ (y1 & z1) ^ z1;
    }

    /// <summary>
    /// Resets all underlying LFSRs to their respective provided states.
    /// </summary>
    /// <param name="stateX">The new state for the first LFSR.</param>
    /// <param name="stateY">The new state for the second LFSR.</param>
    /// <param name="stateZ">The new state for the third LFSR.</param>
    public void Reset(int[] stateX, int[] stateY, int[] stateZ)
    {
        x.Reset(stateX);
        y.Reset(stateY);
        z.Reset(stateZ);
    }
}
