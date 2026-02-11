using System.Numerics;
using System.Runtime.CompilerServices;

namespace Task01.Domain.Services.Lfsr;

/// <summary>
/// Provides a high-performance implementation of a Linear Feedback Shift Register (LFSR)
/// optimized for degrees up to 64 using bit-packed <see cref="ulong"/> state.
/// </summary>
public sealed class Lfsr : ILfsr
{
    private readonly bool[] _feedback;
    private readonly int _degree;
    private readonly ulong _tapMask;
    private readonly ulong _highBitMask;

    private ulong _stateBits;

    /// <summary>
    /// Initializes a new instance of the <see cref="Lfsr"/> class.
    /// </summary>
    /// <param name="feedbackCoefficients">The feedback coefficients defining the connection polynomial.</param>
    /// <param name="initialState">The initial state of the register.</param>
    /// <exception cref="ArgumentException">Thrown when feedback coefficients are empty, state length mismatches feedback length, or if the feedback/state is invalid.</exception>
    /// <exception cref="NotSupportedException">Thrown when the degree exceeds 64.</exception>
    public Lfsr(IEnumerable<bool> feedbackCoefficients, IEnumerable<bool> initialState)
    {
        _feedback = feedbackCoefficients as bool[] ?? feedbackCoefficients.ToArray();
        var initialArray = initialState as bool[] ?? initialState.ToArray();

        if (_feedback.Length == 0)
        {
            throw new ArgumentException("Feedback coefficients cannot be empty.", nameof(feedbackCoefficients));
        }

        if (_feedback.Length > 64)
        {
            throw new NotSupportedException("Bit-packed LFSR supports degree up to 64.");
        }

        if (initialArray.Length != _feedback.Length)
        {
            throw new ArgumentException("Initial state length must match feedback length.", nameof(initialState));
        }

        _degree = _feedback.Length;
        _tapMask = CreateTapMask(_feedback, out var hasTap);

        if (!hasTap)
        {
            throw new ArgumentException("At least one feedback coefficient must be set to true.",
                nameof(feedbackCoefficients));
        }

        _highBitMask = 1UL << (_degree - 1);
        _stateBits = PackState(initialArray, out var hasOne);

        if (!hasOne)
        {
            throw new ArgumentException("Initial state cannot be all zeros.", nameof(initialState));
        }
    }

    /// <inheritdoc />
    public int Degree => _degree;

    /// <inheritdoc />
    public IReadOnlyList<bool> FeedbackCoefficients => _feedback;

    /// <inheritdoc />
    public IReadOnlyList<bool> State => UnpackState(_stateBits, _degree);

    /// <summary>
    /// Advances the LFSR by one step and returns the output bit.
    /// </summary>
    /// <returns>The generated bit.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool NextBit()
    {
        var state = _stateBits;
        var output = NextBitCore(ref state, _tapMask, _highBitMask);
        _stateBits = state;
        return output;
    }

    /// <summary>
    /// Resets the LFSR to a specified state.
    /// </summary>
    /// <param name="state">The new state to load.</param>
    /// <exception cref="ArgumentException">Thrown when the state length is incorrect or the state consists of all zeros.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Reset(IEnumerable<bool> state)
    {
        var candidate = state as bool[] ?? state.ToArray();

        if (candidate.Length != _degree)
        {
            throw new ArgumentException("State length must match feedback length.", nameof(state));
        }

        _stateBits = PackState(candidate, out var hasOne);

        if (!hasOne)
        {
            throw new ArgumentException("State cannot be all zeros.", nameof(state));
        }
    }

    /// <summary>
    /// Generates a sequence of bits by advancing the LFSR multiple times.
    /// </summary>
    /// <param name="count">The number of bits to generate.</param>
    /// <returns>A read-only list of generated bits.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is negative.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public IReadOnlyList<bool> GenerateBits(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return Array.Empty<bool>();
        }

        var result = GC.AllocateUninitializedArray<bool>(count);

        var state = _stateBits;
        var tapMask = _tapMask;
        var highBitMask = _highBitMask;

        var i = 0;
        var len = count;

        while (i + 8 <= len)
        {
            result[i++] = NextBitCore(ref state, tapMask, highBitMask);
            result[i++] = NextBitCore(ref state, tapMask, highBitMask);
            result[i++] = NextBitCore(ref state, tapMask, highBitMask);
            result[i++] = NextBitCore(ref state, tapMask, highBitMask);
            result[i++] = NextBitCore(ref state, tapMask, highBitMask);
            result[i++] = NextBitCore(ref state, tapMask, highBitMask);
            result[i++] = NextBitCore(ref state, tapMask, highBitMask);
            result[i++] = NextBitCore(ref state, tapMask, highBitMask);
        }

        while (i < len)
        {
            result[i++] = NextBitCore(ref state, tapMask, highBitMask);
        }

        _stateBits = state;

        return result;
    }

    /// <summary>
    /// Core logic for calculating the next state and output bit using bitwise operations.
    /// </summary>
    /// <param name="state">Reference to the current state bits.</param>
    /// <param name="tapMask">The bitmask representing feedback taps.</param>
    /// <param name="highBitMask">The bitmask for the most significant bit.</param>
    /// <returns>The output bit.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool NextBitCore(ref ulong state, ulong tapMask, ulong highBitMask)
    {
        var s = state;

        var output = (s & 1UL) != 0;

        var tapsValue = s & tapMask;
        var parity = (BitOperations.PopCount(tapsValue) & 1) != 0;

        s >>= 1;

        if (parity)
        {
            s |= highBitMask;
        }

        state = s;

        return output;
    }

    /// <summary>
    /// Creates a bitmask representing the feedback coefficients.
    /// </summary>
    /// <param name="feedback">The feedback coefficients array.</param>
    /// <param name="hasTap">Out parameter indicating if at least one tap exists.</param>
    /// <returns>A <see cref="ulong"/> bitmask.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static ulong CreateTapMask(bool[] feedback, out bool hasTap)
    {
        ulong mask = 0;
        hasTap = false;

        for (var i = 0; i < feedback.Length; i++)
        {
            if (!feedback[i])
            {
                continue;
            }

            hasTap = true;
            mask |= 1UL << i;
        }

        return mask;
    }

    /// <summary>
    /// Packs a boolean array state into a <see cref="ulong"/>.
    /// </summary>
    /// <param name="state">The state array.</param>
    /// <param name="hasOne">Out parameter indicating if the state contains at least one '1'.</param>
    /// <returns>The packed state bits.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static ulong PackState(bool[] state, out bool hasOne)
    {
        ulong bits = 0;
        hasOne = false;

        for (var i = 0; i < state.Length; i++)
        {
            if (!state[i])
            {
                continue;
            }

            hasOne = true;
            bits |= 1UL << i;
        }

        return bits;
    }

    /// <summary>
    /// Unpacks a <see cref="ulong"/> state into a boolean array.
    /// </summary>
    /// <param name="bits">The packed state bits.</param>
    /// <param name="length">The length of the state.</param>
    /// <returns>The unpacked boolean array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool[] UnpackState(ulong bits, int length)
    {
        var result = GC.AllocateUninitializedArray<bool>(length);

        for (var i = 0; i < length; i++)
        {
            result[i] = ((bits >> i) & 1UL) != 0;
        }

        return result;
    }
}
