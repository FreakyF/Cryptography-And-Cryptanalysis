using System.Runtime.CompilerServices;

namespace Task01;

public sealed class Lfsr : ILfsr
{
    private readonly bool[] _feedback;
    private bool[] _state;

    public Lfsr(IEnumerable<bool> feedbackCoefficients, IEnumerable<bool> initialState)
    {
        _feedback = feedbackCoefficients.ToArray();
        _state = initialState.ToArray();

        if (_feedback.Length == 0)
        {
            throw new ArgumentException("Feedback coefficients cannot be empty.", nameof(feedbackCoefficients));
        }

        if (_state.Length != _feedback.Length)
        {
            throw new ArgumentException("Initial state length must match feedback length.", nameof(initialState));
        }

        if (!_feedback.Any(bit => bit))
        {
            throw new ArgumentException("At least one feedback coefficient must be set to true.",
                nameof(feedbackCoefficients));
        }

        if (!_state.Any(bit => bit))
        {
            throw new ArgumentException("Initial state cannot be all zeros.", nameof(initialState));
        }
    }

    public int Degree => _feedback.Length;
    public IReadOnlyList<bool> FeedbackCoefficients => _feedback;
    public IReadOnlyList<bool> State => _state;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool NextBit()
    {
        var output = _state[0];

        var feedbackBit = _feedback
            .Select((coeff, index) => coeff && _state[index])
            .Aggregate(false, (acc, bit) => acc ^ bit);

        var newState = new bool[_state.Length];
        Array.Copy(_state, 1, newState, 0, _state.Length - 1);
        newState[^1] = feedbackBit;
        _state = newState;

        return output;
    }

    public void Reset(IEnumerable<bool> state)
    {
        var candidate = state.ToArray();

        if (candidate.Length != _feedback.Length)
        {
            throw new ArgumentException("State length must match feedback length.", nameof(state));
        }

        if (!candidate.Any(bit => bit))
        {
            throw new ArgumentException("State cannot be all zeros.", nameof(state));
        }

        _state = candidate;
    }

    public IReadOnlyList<bool> GenerateBits(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var result = new bool[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = NextBit();
        }

        return result;
    }
}