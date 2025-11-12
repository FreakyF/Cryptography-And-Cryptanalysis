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
    private const int IterationCount = 100_000_000;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public HeuristicResult Analyze(string cipherText, string referenceText, string alphabet)
    {
        if (string.IsNullOrEmpty(alphabet))
            throw new ArgumentException("Alphabet must be provided", nameof(alphabet));
        if (!alphabet.Equals("ABCDEFGHIJKLMNOPQRSTUVWXYZ", StringComparison.Ordinal))
            throw new ArgumentException("Alphabet must be A–Z for the fast path.", nameof(alphabet));

        var normalizedCipher = textNormalizer.Normalize(cipherText);
        if (normalizedCipher.Length == 0)
            return new HeuristicResult(alphabet, string.Empty, double.NegativeInfinity);

        var normalizedReference = textNormalizer.Normalize(referenceText);
        var model = BigramLanguageModel.CreateAZ(alphabet, normalizedReference, SmoothingConstant);

        var c = normalizedCipher.AsSpan();
        var cipherIdx = new byte[c.Length];
        for (int i = 0; i < c.Length; i++)
            cipherIdx[i] = (byte)(c[i] - 'A');

        var counts = new int[26 * 26];
        if (cipherIdx.Length >= 2)
        {
            ref int baseCnt = ref MemoryMarshal.GetArrayDataReference(counts);
            for (int i = 1; i < cipherIdx.Length; i++)
            {
                int r = cipherIdx[i - 1];
                int col = cipherIdx[i];
                Unsafe.Add(ref baseCnt, r * 26 + col)++;
            }
        }

        var rng = Random.Shared;
        var permArr = alphabet.ToCharArray();
        for (int i = permArr.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (permArr[i], permArr[j]) = (permArr[j], permArr[i]);
        }
        Span<char> perm = permArr.AsSpan();

        Span<byte> invPos = stackalloc byte[26];
        for (int i = 0; i < 26; i++)
            invPos[(byte)(perm[i] - 'A')] = (byte)i;

        var bestPerm = alphabet.ToCharArray();

        double currentScore = model.ScoreFromCounts(invPos, counts);
        double bestScore = currentScore;

        var rnd = Random.Shared;

        for (int it = 0; it < IterationCount; it++)
        {
            int i = rnd.Next(26);
            int j = rnd.Next(25);
            if (j >= i) j++;

            double proposalScore = model.ProposedScoreDelta(invPos, counts, i, j, currentScore);

            double delta = proposalScore - currentScore;
            bool accept = delta >= 0d || Math.Log(rnd.NextDouble()) <= delta;
            if (!accept) continue;

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
        public static BigramLanguageModel CreateAZ(string alphabet, string referenceText, double alpha)
        {
            const int size = 26;

            var counts = new double[size * size];

            if (!string.IsNullOrEmpty(referenceText))
            {
                var s = referenceText.AsSpan();
                if (s.Length >= 2)
                {
                    int prev = s[0] - 'A';
                    int prevRow = (uint)prev < 26u ? prev * size : -1;
                    for (int i = 1; i < s.Length; i++)
                    {
                        int col = s[i] - 'A';
                        if (prevRow >= 0 && (uint)col < 26u) counts[prevRow + col] += 1d;
                        prevRow = (uint)col < 26u ? (col * size) : -1;
                    }
                }
            }

            double total = 0d;
            for (int i = 0; i < counts.Length; i++) { counts[i] += alpha; total += counts[i]; }

            var log = new double[counts.Length];
            double invTotal = 1d / total;
            for (int i = 0; i < counts.Length; i++) log[i] = Math.Log(counts[i] * invTotal);

            var rowOff = new int[size];
            for (int k = 0; k < size; k++) rowOff[k] = k * size;

            return new BigramLanguageModel(log, rowOff);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public double ScoreFromCounts(ReadOnlySpan<byte> invPos, int[] counts)
        {
            ref double log0 = ref MemoryMarshal.GetArrayDataReference(_log);
            ref int cnt0 = ref MemoryMarshal.GetArrayDataReference(counts);

            double sum = 0d;

            for (int p = 0; p < 26; p++)
            {
                int row = _rowOff[invPos[p]];
                int cntRowOff = p * 26;
                for (int q = 0; q < 26; q++)
                {
                    int c = Unsafe.Add(ref cnt0, cntRowOff + q);
                    if (c == 0) continue;
                    int col = invPos[q];
                    sum += Unsafe.Add(ref log0, row + col) * c;
                }
            }

            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public double ProposedScoreDelta(ReadOnlySpan<byte> invPos, int[] counts, int iPos, int jPos, double currentScore)
        {
            int x = -1, y = -1;
            for (int ch = 0; ch < 26; ch++)
            {
                int pos = invPos[ch];
                if (pos == iPos) x = ch;
                else if (pos == jPos) y = ch;
            }

            if ((uint)x >= 26u || (uint)y >= 26u)
                return currentScore;

            ref double log0 = ref MemoryMarshal.GetArrayDataReference(_log);
            ref int cnt0 = ref MemoryMarshal.GetArrayDataReference(counts);

            double delta = 0d;

            int cntRowX = x * 26;
            int cntRowY = y * 26;
            int rowI = _rowOff[iPos];
            int rowJ = _rowOff[jPos];
            for (int q = 0; q < 26; q++)
            {
                int cXq = Unsafe.Add(ref cnt0, cntRowX + q);
                if (cXq != 0)
                {
                    int col = invPos[q];
                    delta += (Unsafe.Add(ref log0, rowJ + col) - Unsafe.Add(ref log0, rowI + col)) * cXq;
                }

                int cYq = Unsafe.Add(ref cnt0, cntRowY + q);
                if (cYq != 0)
                {
                    int col = invPos[q];
                    delta += (Unsafe.Add(ref log0, rowI + col) - Unsafe.Add(ref log0, rowJ + col)) * cYq;
                }
            }

            for (int p = 0; p < 26; p++)
            {
                if (p == x || p == y) continue;

                int cpx = Unsafe.Add(ref cnt0, p * 26 + x);
                if (cpx != 0)
                {
                    int row = _rowOff[invPos[p]];
                    delta += (Unsafe.Add(ref log0, row + jPos) - Unsafe.Add(ref log0, row + iPos)) * cpx;
                }

                int cpy = Unsafe.Add(ref cnt0, p * 26 + y);
                if (cpy != 0)
                {
                    int row = _rowOff[invPos[p]];
                    delta += (Unsafe.Add(ref log0, row + iPos) - Unsafe.Add(ref log0, row + jPos)) * cpy;
                }
            }

            return currentScore + delta;
        }
    }
}
