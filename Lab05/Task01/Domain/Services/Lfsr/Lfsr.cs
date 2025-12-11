using System.Numerics;
using System.Runtime.CompilerServices;

namespace Task01.Domain.Services.Lfsr;

public sealed class Lfsr : ILfsr
{
    private readonly bool[] _feedback;
    private readonly int _degree;
    private readonly ulong _tapMask;
    private readonly ulong _highBitMask;

    private ulong _stateBits;

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

    public int Degree => _degree;
    public IReadOnlyList<bool> FeedbackCoefficients => _feedback;
    public IReadOnlyList<bool> State => UnpackState(_stateBits, _degree);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool NextBit()
    {
        var state = _stateBits;
        var output = NextBitCore(ref state, _tapMask, _highBitMask);
        _stateBits = state;
        return output;
    }

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

        // Unroll 8×
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

        // Ewentualny ogon < 8 bitów
        while (i < len)
        {
            result[i++] = NextBitCore(ref state, tapMask, highBitMask);
        }

        _stateBits = state;

        return result;
    }

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
