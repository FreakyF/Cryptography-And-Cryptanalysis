namespace Lab06.Domain.Generators;

public class Lfsr(int degree, int[] taps, int[] initialState) : ILfsr
{
    private int[] _currentState = [..initialState];

    public int Degree { get; } = degree;

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

    public void Reset(int[] startState)
    {
        if (startState.Length != Degree)
        {
            throw new ArgumentException("Invalid state length");
        }

        _currentState = startState.ToArray();
    }
}