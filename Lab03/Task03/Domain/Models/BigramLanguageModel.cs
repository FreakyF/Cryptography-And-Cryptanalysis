using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Task03.Domain.Models;

public sealed class BigramLanguageModel
{
    private readonly double[] _log;
    private readonly int[] _rowOff;

    private BigramLanguageModel(double[] log, int[] rowOff)
    {
        _log = log;
        _rowOff = rowOff;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BigramLanguageModel CreateFromBigramsText(string bigramsText, double alpha)
    {
        const int size = 26;
        var counts = new double[size * size];

        if (!string.IsNullOrEmpty(bigramsText))
        {
            var lines = bigramsText.Split(new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var line in lines)
            {
                if (line.Length < 4)
                {
                    continue;
                }

                var span = line.AsSpan().Trim();
                if (span.Length < 4)
                {
                    continue;
                }

                var c0 = char.ToUpperInvariant(span[0]);
                var c1 = char.ToUpperInvariant(span[1]);
                if (c0 < 'A' || c0 > 'Z' || c1 < 'A' || c1 > 'Z')
                {
                    continue;
                }

                var spaceIdx = span.IndexOf(' ');
                if (spaceIdx < 0 || spaceIdx + 1 >= span.Length)
                {
                    continue;
                }

                var countSpan = span[(spaceIdx + 1)..].Trim();
                if (!long.TryParse(countSpan, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out var cnt) || cnt <= 0)
                {
                    continue;
                }

                var row = c0 - 'A';
                var col = c1 - 'A';
                counts[row * size + col] += cnt;
            }
        }

        var total = 0d;
        for (var i = 0; i < counts.Length; i++)
        {
            counts[i] += alpha;
            total += counts[i];
        }

        var log = new double[counts.Length];
        var invTotal = 1d / total;
        for (var i = 0; i < counts.Length; i++)
        {
            log[i] = Math.Log(counts[i] * invTotal);
        }

        var rowOff = new int[size];
        for (var k = 0; k < size; k++)
        {
            rowOff[k] = k * size;
        }

        return new BigramLanguageModel(log, rowOff);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public double ScoreFromCounts(ReadOnlySpan<byte> invPos, int[] counts)
    {
        ref var log0 = ref MemoryMarshal.GetArrayDataReference(_log);
        ref var cnt0 = ref MemoryMarshal.GetArrayDataReference(counts);

        var sum = 0d;
        for (var p = 0; p < 26; p++)
        {
            var row = _rowOff[invPos[p]];
            var cntRowOff = p * 26;
            for (var q = 0; q < 26; q++)
            {
                var c = Unsafe.Add(ref cnt0, cntRowOff + q);
                if (c == 0)
                {
                    continue;
                }

                int col = invPos[q];
                sum += Unsafe.Add(ref log0, row + col) * c;
            }
        }

        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public double ProposedScoreDelta(
        ReadOnlySpan<byte> invPos,
        ReadOnlySpan<char> perm,
        int[] counts,
        int iPos,
        int jPos,
        double currentScore)
    {
        var x = perm[iPos] - 'A';
        var y = perm[jPos] - 'A';
        if ((uint)x >= 26u || (uint)y >= 26u)
        {
            return currentScore;
        }

        ref var log0 = ref MemoryMarshal.GetArrayDataReference(_log);
        ref var cnt0 = ref MemoryMarshal.GetArrayDataReference(counts);

        var delta = 0d;

        var cntRowX = x * 26;
        var cntRowY = y * 26;
        var rowPx = _rowOff[iPos];
        var rowPy = _rowOff[jPos];

        for (var q = 0; q < 26; q++)
        {
            if (q == x || q == y)
            {
                continue;
            }

            int colMq = invPos[q];

            var cXq = Unsafe.Add(ref cnt0, cntRowX + q);
            if (cXq != 0)
            {
                delta += (Unsafe.Add(ref log0, rowPy + colMq) - Unsafe.Add(ref log0, rowPx + colMq)) * cXq;
            }

            var cYq = Unsafe.Add(ref cnt0, cntRowY + q);
            if (cYq != 0)
            {
                delta += (Unsafe.Add(ref log0, rowPx + colMq) - Unsafe.Add(ref log0, rowPy + colMq)) * cYq;
            }
        }

        for (var p = 0; p < 26; p++)
        {
            if (p == x || p == y)
            {
                continue;
            }

            var rowMp = _rowOff[invPos[p]];

            var cpx = Unsafe.Add(ref cnt0, p * 26 + x);
            if (cpx != 0)
            {
                delta += (Unsafe.Add(ref log0, rowMp + jPos) - Unsafe.Add(ref log0, rowMp + iPos)) * cpx;
            }

            var cpy = Unsafe.Add(ref cnt0, p * 26 + y);
            if (cpy != 0)
            {
                delta += (Unsafe.Add(ref log0, rowMp + iPos) - Unsafe.Add(ref log0, rowMp + jPos)) * cpy;
            }
        }

        var Cxx = Unsafe.Add(ref cnt0, cntRowX + x);
        if (Cxx != 0)
        {
            delta += (Unsafe.Add(ref log0, rowPy + jPos) - Unsafe.Add(ref log0, rowPx + iPos)) * Cxx;
        }

        var Cyy = Unsafe.Add(ref cnt0, cntRowY + y);
        if (Cyy != 0)
        {
            delta += (Unsafe.Add(ref log0, rowPx + iPos) - Unsafe.Add(ref log0, rowPy + jPos)) * Cyy;
        }

        var Cxy = Unsafe.Add(ref cnt0, cntRowX + y);
        if (Cxy != 0)
        {
            delta += (Unsafe.Add(ref log0, rowPy + iPos) - Unsafe.Add(ref log0, rowPx + jPos)) * Cxy;
        }

        var Cyx = Unsafe.Add(ref cnt0, cntRowY + x);
        if (Cyx != 0)
        {
            delta += (Unsafe.Add(ref log0, rowPx + jPos) - Unsafe.Add(ref log0, rowPy + iPos)) * Cyx;
        }

        return currentScore + delta;
    }
}