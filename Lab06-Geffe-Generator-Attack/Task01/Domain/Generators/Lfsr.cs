namespace Lab06.Domain.Generators;

/// <summary>
/// Represents a Linear Feedback Shift Register (LFSR) generator implementation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Lfsr"/> class.
/// </remarks>
/// <param name="degree">The degree of the characteristic polynomial.</param>
/// <param name="taps">The feedback tap positions.</param>
/// <param name="initialState">The initial state of the register.</param>
public class Lfsr(int degree, int[] taps, int[] initialState) : ILfsr
{
    /// <summary>
    /// The current internal state of the shift register.
    /// </summary>
    private int[] _currentState = [..initialState];

    /// <summary>
    /// Gets the degree of the LFSR.
    /// </summary>
    public int Degree { get; } = degree;

    /// <summary>
    /// Generates the next bit based on the linear feedback function.
    /// </summary>
    /// <returns>The next bit in the sequence (0 or 1).</returns>
    public int NextBit()
    {
        var outputBit = _currentState[0];
        var feedbackBit = taps.Aggregate(0, (current, tapIndex) => current ^ _currentState[tapIndex]);

        for (var i = 0; i < Degree - 1; i++)
        {
            _currentState[i] = _currentState[i + 1];
        }

        _currentState[Degree - 1] = feedbackBit;

        return outputBit;
    }

    /// <summary>
    /// Resets the LFSR state to the specified start state.
    /// </summary>
    /// <param name="startState">The new state array to load into the register.</param>
    /// <exception cref="ArgumentException">Thrown when the length of <paramref name="startState"/> does not match the LFSR degree.</exception>
    public void Reset(int[] startState)
    {
        if (startState.Length != Degree)
        {
            throw new ArgumentException("Invalid state length");
        }

        _currentState = startState.ToArray();
    }
}
