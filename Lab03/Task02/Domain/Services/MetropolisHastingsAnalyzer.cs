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

        var rng = new Xoshiro256(((ulong)c.Length << 32) ^ (ulong)Environment.TickCount64);

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
}