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
            throw new ArgumentException("Alphabet must be provided", nameof(alphabet));
        if (!alphabet.Equals("ABCDEFGHIJKLMNOPQRSTUVWXYZ", StringComparison.Ordinal))
            throw new ArgumentException("Alphabet must be A–Z for the fast path.", nameof(alphabet));

        var normalizedCipher = textNormalizer.Normalize(cipherText);
        if (normalizedCipher.Length == 0)
            return new HeuristicResult(alphabet, string.Empty, double.NegativeInfinity);

        var model = BigramLanguageModel.CreateFromBigramsText(referenceText, SmoothingConstant);

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
        public static BigramLanguageModel CreateFromBigramsText(string bigramsText, double alpha)
        {
            const int size = 26;
            var counts = new double[size * size];

            if (!string.IsNullOrEmpty(bigramsText))
            {
                var lines = bigramsText.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var line in lines)
                {
                    if (line.Length < 4) continue;

                    var span = line.AsSpan().Trim();
                    if (span.Length < 4) continue;

                    char c0 = char.ToUpperInvariant(span[0]);
                    char c1 = char.ToUpperInvariant(span[1]);
                    if (c0 < 'A' || c0 > 'Z' || c1 < 'A' || c1 > 'Z') continue;

                    int spaceIdx = span.IndexOf(' ');
                    if (spaceIdx < 0 || spaceIdx + 1 >= span.Length) continue;

                    var countSpan = span[(spaceIdx + 1)..].Trim();
                    if (!long.TryParse(countSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cnt) || cnt <= 0)
                        continue;

                    int row = c0 - 'A';
                    int col = c1 - 'A';
                    if ((uint)row >= 26u || (uint)col >= 26u) continue;

                    counts[row * size + col] += cnt;
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
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] += alpha;
                total += counts[i];
            }

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
            Span<byte> invPosProposal = stackalloc byte[26];
            invPos.CopyTo(invPosProposal);

            for (int ch = 0; ch < 26; ch++)
            {
                byte pos = invPosProposal[ch];
                if (pos == iPos) invPosProposal[ch] = (byte)jPos;
                else if (pos == jPos) invPosProposal[ch] = (byte)iPos;
            }

            return ScoreFromCounts(invPosProposal, counts);
        }
    }
}