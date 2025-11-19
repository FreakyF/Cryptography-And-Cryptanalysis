using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Task03.Domain.Models;

public sealed class BigramLanguageModel
{
    private readonly float[] _w;

    /// <summary>Initializes the model with normalized bigram weights.</summary>
    /// <param name="w">The weight array to retain.</param>
    private BigramLanguageModel(float[] w)
    {
        _w = w;
    }

    /// <summary>Builds a language model by parsing bigram counts and applying additive smoothing.</summary>
    /// <param name="bigramsText">The text listing bigrams and their counts.</param>
    /// <param name="alpha">The smoothing value added to each count.</param>
    /// <returns>A normalized bigram language model.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BigramLanguageModel CreateFromBigramsText(string bigramsText, double alpha)
    {
        const int size = 26;
        var phi = new double[size * size];

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

                var r = char.ToUpperInvariant(span[0]) - 'A';
                var c = char.ToUpperInvariant(span[1]) - 'A';
                if ((uint)r >= 26u || (uint)c >= 26u)
                {
                    continue;
                }

                var sp = span.IndexOf(' ');
                if (sp < 0 || sp + 1 >= span.Length)
                {
                    continue;
                }

                var countSpan = span[(sp + 1)..].Trim();
                if (!long.TryParse(countSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cnt) ||
                    cnt <= 0)
                {
                    continue;
                }

                phi[r * size + c] += cnt;
            }
        }

        var maxPhi = 0d;
        for (var i = 0; i < phi.Length; i++)
        {
            phi[i] += alpha;
            if (phi[i] > maxPhi)
            {
                maxPhi = phi[i];
            }
        }

        if (maxPhi == 0d)
        {
            maxPhi = 1d;
        }

        var w = new float[phi.Length];
        var invMax = (float)(1d / maxPhi);
        for (var i = 0; i < phi.Length; i++)
        {
            w[i] = (float)phi[i] * invMax;
        }

        return new BigramLanguageModel(w);
    }

    /// <summary>Computes the log-likelihood score for a permutation using cached bigram counts.</summary>
    /// <param name="invPos">The inverse permutation positions for each alphabet index.</param>
    /// <param name="counts">The bigram occurrence counts from the cipher text.</param>
    /// <returns>The score representing the quality of the permutation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public double ScoreFromCounts(ReadOnlySpan<byte> invPos, int[] counts)
    {
        ref var wBase = ref MemoryMarshal.GetArrayDataReference(_w);
        ref var cntBase = ref MemoryMarshal.GetArrayDataReference(counts);

        var sum = 0d;

        for (var p = 0; p < 26; p++)
        {
            var row = invPos[p] * 26;
            var cntRow = p * 26;

            for (var q = 0; q < 26; q++)
            {
                var c = Unsafe.Add(ref cntBase, cntRow + q);
                if (c == 0)
                {
                    continue;
                }

                int col = invPos[q];
                sum += (double)Unsafe.Add(ref wBase, row + col) * c;
            }
        }

        return sum;
    }

    /// <summary>Estimates the new score when swapping two permutation positions without recomputing the entire score.</summary>
    /// <param name="invPos">The inverse permutation lookup.</param>
    /// <param name="perm">The current permutation.</param>
    /// <param name="counts">The bigram occurrence counts.</param>
    /// <param name="iPos">The first position to swap.</param>
    /// <param name="jPos">The second position to swap.</param>
    /// <param name="currentScore">The current score before applying the swap.</param>
    /// <returns>The updated score reflecting the proposed swap.</returns>
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

        ref var wBase = ref MemoryMarshal.GetArrayDataReference(_w);
        ref var cntBase = ref MemoryMarshal.GetArrayDataReference(counts);

        var rowPx = iPos * 26;
        var rowPy = jPos * 26;
        var cntRowX = x * 26;
        var cntRowY = y * 26;

        var delta = 0d;

        for (var q = 0; q < 26; q++)
        {
            if (q == x || q == y)
            {
                continue;
            }

            int colMq = invPos[q];

            var cXq = Unsafe.Add(ref cntBase, cntRowX + q);
            if (cXq != 0)
            {
                var wPy = Unsafe.Add(ref wBase, rowPy + colMq);
                var wPx = Unsafe.Add(ref wBase, rowPx + colMq);
                delta += (double)(wPy - wPx) * cXq;
            }

            var cYq = Unsafe.Add(ref cntBase, cntRowY + q);
            if (cYq != 0)
            {
                var wPx2 = Unsafe.Add(ref wBase, rowPx + colMq);
                var wPy2 = Unsafe.Add(ref wBase, rowPy + colMq);
                delta += (double)(wPx2 - wPy2) * cYq;
            }
        }

        for (var p = 0; p < 26; p++)
        {
            if (p == x || p == y)
            {
                continue;
            }

            var rowMp = invPos[p] * 26;
            var cntRowP = p * 26;

            var cpx = Unsafe.Add(ref cntBase, cntRowP + x);
            if (cpx != 0)
            {
                var wMj = Unsafe.Add(ref wBase, rowMp + jPos);
                var wMi = Unsafe.Add(ref wBase, rowMp + iPos);
                delta += (double)(wMj - wMi) * cpx;
            }

            var cpy = Unsafe.Add(ref cntBase, cntRowP + y);
            if (cpy != 0)
            {
                var wMi2 = Unsafe.Add(ref wBase, rowMp + iPos);
                var wMj2 = Unsafe.Add(ref wBase, rowMp + jPos);
                delta += (double)(wMi2 - wMj2) * cpy;
            }
        }

        // NAROŻA
        var Cxx = Unsafe.Add(ref cntBase, cntRowX + x);
        if (Cxx != 0)
        {
            var wPyj = Unsafe.Add(ref wBase, rowPy + jPos);
            var wPxi = Unsafe.Add(ref wBase, rowPx + iPos);
            delta += (double)(wPyj - wPxi) * Cxx;
        }

        var Cyy = Unsafe.Add(ref cntBase, cntRowY + y);
        if (Cyy != 0)
        {
            var wPxi2 = Unsafe.Add(ref wBase, rowPx + iPos);
            var wPyj2 = Unsafe.Add(ref wBase, rowPy + jPos);
            delta += (double)(wPxi2 - wPyj2) * Cyy;
        }

        var Cxy = Unsafe.Add(ref cntBase, cntRowX + y);
        if (Cxy != 0)
        {
            var wPyi = Unsafe.Add(ref wBase, rowPy + iPos);
            var wPxj = Unsafe.Add(ref wBase, rowPx + jPos);
            delta += (double)(wPyi - wPxj) * Cxy;
        }

        var Cyx = Unsafe.Add(ref cntBase, cntRowY + x);
        if (Cyx != 0)
        {
            var wPxj2 = Unsafe.Add(ref wBase, rowPx + jPos);
            var wPyi2 = Unsafe.Add(ref wBase, rowPy + iPos);
            delta += (double)(wPxj2 - wPyi2) * Cyx;
        }

        return currentScore + delta;
    }
}