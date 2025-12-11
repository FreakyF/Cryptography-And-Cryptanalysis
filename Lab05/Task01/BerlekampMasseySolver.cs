using System.Runtime.CompilerServices;

namespace Task01;

[SkipLocalsInit]
public sealed class BerlekampMasseySolver : IBerlekampMasseySolver
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public BerlekampMasseyResult Solve(IReadOnlyList<bool> sequence)
    {
        if (sequence == null)
        {
            throw new ArgumentNullException(nameof(sequence));
        }

        var n = sequence.Count;

        if (n == 0)
        {
            var coeffs = new[] { true };
            return new BerlekampMasseyResult(coeffs, 0);
        }

        if (n <= 63)
        {
            return SolvePacked(sequence, n);
        }

        return SolveArray(sequence, n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static BerlekampMasseyResult SolvePacked(IReadOnlyList<bool> sequence, int n)
    {
        bool[] s;

        if (sequence is bool[] arr)
        {
            s = arr;
        }
        else
        {
            s = GC.AllocateUninitializedArray<bool>(n);
            for (var i = 0; i < n; i++)
            {
                s[i] = sequence[i];
            }
        }

        ulong c = 1;
        ulong b = 1;

        var l = 0;
        var m = -1;

        for (var index = 0; index < n; index++)
        {
            var discrepancy = s[index];

            for (var i = 1; i <= l; i++)
            {
                if (((c >> i) & 1UL) != 0 && s[index - i])
                {
                    discrepancy ^= true;
                }
            }

            if (!discrepancy)
            {
                continue;
            }

            var previousC = c;
            var delta = index - m;

            c ^= b << delta;

            if (2 * l <= index)
            {
                l = index + 1 - l;
                b = previousC;
                m = index;
            }
        }

        var resultLength = l + 1;
        var resultCoeffs = GC.AllocateUninitializedArray<bool>(resultLength);

        for (var i = 0; i < resultLength; i++)
        {
            resultCoeffs[i] = ((c >> i) & 1UL) != 0;
        }

        return new BerlekampMasseyResult(resultCoeffs, l);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static BerlekampMasseyResult SolveArray(IReadOnlyList<bool> sequence, int n)
    {
        Span<bool> c = n <= 64 ? stackalloc bool[n] : GC.AllocateUninitializedArray<bool>(n);
        Span<bool> b = n <= 64 ? stackalloc bool[n] : GC.AllocateUninitializedArray<bool>(n);
        Span<bool> temp = n <= 64 ? stackalloc bool[n] : GC.AllocateUninitializedArray<bool>(n);

        c.Clear();
        b.Clear();

        c[0] = true;
        b[0] = true;

        var l = 0;
        var m = -1;
        var cLen = 1;
        var bLen = 1;

        for (var index = 0; index < n; index++)
        {
            var discrepancy = sequence[index];

            for (var i = 1; i <= l; i++)
            {
                if (c[i] && sequence[index - i])
                {
                    discrepancy ^= true;
                }
            }

            if (!discrepancy)
            {
                continue;
            }

            for (var i = 0; i < cLen; i++)
            {
                temp[i] = c[i];
            }

            var delta = index - m;
            var maxIndex = delta + bLen;
            if (maxIndex > cLen)
            {
                for (var i = cLen; i < maxIndex; i++)
                {
                    c[i] = false;
                }

                cLen = maxIndex;
            }

            var limit = bLen;
            if (delta + limit > n)
            {
                limit = n - delta;
            }

            for (var i = 0; i < limit; i++)
            {
                c[delta + i] ^= b[i];
            }

            if (2 * l <= index)
            {
                l = index + 1 - l;

                for (var i = 0; i < cLen; i++)
                {
                    b[i] = temp[i];
                }

                bLen = cLen;
                m = index;
            }
        }

        var resultLength = l + 1;
        var resultCoeffs = GC.AllocateUninitializedArray<bool>(resultLength);

        for (var i = 0; i < resultLength; i++)
        {
            resultCoeffs[i] = c[i];
        }

        return new BerlekampMasseyResult(resultCoeffs, l);
    }
}
