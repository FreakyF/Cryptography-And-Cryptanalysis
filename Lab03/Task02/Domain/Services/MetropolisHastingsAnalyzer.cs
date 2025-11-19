using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Task02.Domain.Abstractions;
using Task02.Domain.Models;

namespace Task02.Domain.Services;

public sealed class MetropolisHastingsAnalyzer(
    ITextNormalizer textNormalizer,
    ISubstitutionCipher cipher)
    : IHeuristicAnalyzer
{
    private const double SmoothingConstant = 0.01d;
    private const int IterationCount = 500_000;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public HeuristicResult Analyze(string cipherText, string referenceText, string alphabet)
    {
        if (string.IsNullOrEmpty(alphabet))
        {
            throw new ArgumentException("Alphabet must be provided", nameof(alphabet));
        }

        if (!alphabet.Equals("ABCDEFGHIJKLMNOPQRSTUVWXYZ", StringComparison.Ordinal))
        {
            throw new ArgumentException("Alphabet must be A–Z for the fast path.", nameof(alphabet));
        }

        var normalizedCipher = textNormalizer.Normalize(cipherText);
        if (normalizedCipher.Length == 0)
        {
            return new HeuristicResult(alphabet, string.Empty, double.NegativeInfinity);
        }

        var model = BigramLanguageModel.CreateFromBigramsText(referenceText, SmoothingConstant);

        var c = normalizedCipher.AsSpan();
        var cipherIdx = new byte[c.Length];
        for (var i = 0; i < c.Length; i++)
        {
            cipherIdx[i] = (byte)(c[i] - 'A');
        }

        var counts = new int[26 * 26];
        if (cipherIdx.Length >= 2)
        {
            ref var baseCnt = ref MemoryMarshal.GetArrayDataReference(counts);
            for (var i = 1; i < cipherIdx.Length; i++)
            {
                int r = cipherIdx[i - 1];
                int col = cipherIdx[i];
                Unsafe.Add(ref baseCnt, r * 26 + col)++;
            }
        }

        var rng = new FastRng(((ulong)c.Length << 32) ^ (ulong)Environment.TickCount64);

        var permArr = alphabet.ToCharArray();
        for (var i = permArr.Length - 1; i > 0; i--)
        {
            var j = rng.NextInt(i + 1);
            (permArr[i], permArr[j]) = (permArr[j], permArr[i]);
        }

        var perm = permArr.AsSpan();

        Span<byte> invPos = stackalloc byte[26];
        for (var i = 0; i < 26; i++)
        {
            invPos[(byte)(perm[i] - 'A')] = (byte)i;
        }

        var bestPerm = alphabet.ToCharArray();

        var currentScore = model.ScoreFromCounts(invPos, counts);
        var bestScore = currentScore;

        for (var it = 0; it < IterationCount; it++)
        {
            var i = rng.NextInt(26);
            var j = rng.NextInt(25);
            if (j >= i)
            {
                j++;
            }

            var proposalScore = model.ProposedScoreDelta(invPos, perm, counts, i, j, currentScore);
            var delta = proposalScore - currentScore;

            bool accept;
            if (delta >= 0d)
            {
                accept = true;
            }
            else
            {
                var u = (float)rng.NextDouble();
                var logu = MathF.Log(u);
                accept = logu <= delta;
            }

            if (!accept)
            {
                continue;
            }

            (perm[i], perm[j]) = (perm[j], perm[i]);
            invPos[(byte)(perm[i] - 'A')] = (byte)i;
            invPos[(byte)(perm[j] - 'A')] = (byte)j;

            currentScore = proposalScore;

            if (proposalScore > bestScore)
            {
                bestScore = proposalScore;
                perm.CopyTo(bestPerm);
            }
        }

        var bestPermutation = new string(bestPerm);
        var bestPlainText = cipher.Decrypt(normalizedCipher, alphabet, bestPermutation);
        return new HeuristicResult(bestPermutation, bestPlainText, bestScore);
    }

    private struct FastRng
    {
        private ulong _s0, _s1, _s2, _s3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong RotL(ulong x, int k)
        {
            return BitOperations.RotateLeft(x, k);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong SplitMix64(ref ulong x)
        {
            x += 0x9E3779B97F4A7C15ul;
            var z = x;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9ul;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBul;
            return z ^ (z >> 31);
        }

        public FastRng(ulong seed)
        {
            _s0 = _s1 = _s2 = _s3 = 0;
            var sm = seed;
            _s0 = SplitMix64(ref sm);
            _s1 = SplitMix64(ref sm);
            _s2 = SplitMix64(ref sm);
            _s3 = SplitMix64(ref sm);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Next64()
        {
            var s0 = _s0;
            var s1 = _s1;
            var s2 = _s2;
            var s3 = _s3;

            var result = RotL(s1 * 5, 7) * 9;

            var t = s1 << 17;

            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;

            s2 ^= t;
            s3 = RotL(s3, 45);

            _s0 = s0;
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int exclusiveMax)
        {
            return (int)((Next64() * (ulong)exclusiveMax) >> 64);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble()
        {
            return (Next64() >> 11) * (1.0 / (1ul << 53));
        }
    }

    private sealed class BigramLanguageModel
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
}