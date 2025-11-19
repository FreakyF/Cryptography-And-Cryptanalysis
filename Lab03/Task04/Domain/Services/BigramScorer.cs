using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Task04.Domain.Abstractions;
using Task04.Domain.Models;

namespace Task04.Domain.Services;

public sealed class BigramScorer : IBigramScorer
{
    public BigramWeights LoadWeights(string bigramsText, double alpha = 0.01)
    {
        const int size = 26;
        var phi = new double[size * size];

        if (!string.IsNullOrEmpty(bigramsText))
        {
            var lines = bigramsText.Split(
                ['\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var line in lines)
            {
                if (TryParseBigramLine(line.AsSpan(), out var r, out var c, out var cnt))
                {
                    phi[r * size + c] += cnt;
                }
            }
        }

        var maxPhi = ApplySmoothingAndGetMax(phi, alpha);

        const double eps = 1e-12;
        if (maxPhi < eps)
        {
            maxPhi = 1d;
        }

        var w = new float[phi.Length];
        var invMax = (float)(1d / maxPhi);
        for (var i = 0; i < phi.Length; i++)
        {
            w[i] = (float)phi[i] * invMax;
        }

        return new BigramWeights(w);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public double Score(string normalizedText, BigramWeights weights)
    {
        if (string.IsNullOrEmpty(normalizedText))
            return 0d;

        var s = normalizedText.AsSpan();
        if (s.Length < 2)
            return 0d;

        ref var w0 = ref MemoryMarshal.GetArrayDataReference(weights.Weights);

        var sum = 0d;
        var prev = s[0] - 'A';

        for (var i = 1; i < s.Length; i++)
        {
            var cur = s[i] - 'A';
            if ((uint)prev < 26u && (uint)cur < 26u)
                sum += Unsafe.Add(ref w0, prev * 26 + cur);

            prev = cur;
        }

        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseBigramLine(ReadOnlySpan<char> span, out int r, out int c, out long cnt)
    {
        r = c = -1;
        cnt = 0;

        span = span.Trim();
        if (span.Length < 4)
            return false;

        r = char.ToUpperInvariant(span[0]) - 'A';
        c = char.ToUpperInvariant(span[1]) - 'A';
        if ((uint)r >= 26u || (uint)c >= 26u)
            return false;

        int sp = span.IndexOf(' ');
        if (sp < 0 || sp + 1 >= span.Length)
            return false;

        var countSpan = span[(sp + 1)..].Trim();
        if (!long.TryParse(countSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out cnt) || cnt <= 0)
            return false;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ApplySmoothingAndGetMax(double[] phi, double alpha)
    {
        var maxPhi = 0d;
        for (var i = 0; i < phi.Length; i++)
        {
            var v = phi[i] + alpha;
            phi[i] = v;
            if (v > maxPhi) maxPhi = v;
        }

        return maxPhi;
    }
}