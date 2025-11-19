using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Task03.Domain.Abstractions;
using Task03.Domain.Models;

namespace Task03.Domain.Services;

public sealed class SimulatedAnnealingAnalyzer(
    ITextNormalizer textNormalizer,
    ISubstitutionCipher cipher)
    : IHeuristicAnalyzer
{
    private const int IterationCount = 500_000;
    private const int RestartCount = 8;
    private const double T0 = 5.0;
    private const double Alpha = 0.9995;
    private const double Smoothing = 0.01;

    /// <summary>Applies a simulated annealing heuristic to recover the best permutation and plaintext for the cipher text.</summary>
    /// <param name="cipherText">The cipher text to analyze.</param>
    /// <param name="referenceText">The reference corpus used to compute bigram probabilities.</param>
    /// <param name="alphabet">The plaintext alphabet describing the permutation domain.</param>
    /// <returns>The heuristic result containing the best permutation, plaintext, and score.</returns>
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

        var model = BigramLanguageModel.CreateFromBigramsText(referenceText, Smoothing);

        var s = normalizedCipher.AsSpan();
        var cipherIdx = new byte[s.Length];
        for (var i = 0; i < s.Length; i++)
        {
            cipherIdx[i] = (byte)(s[i] - 'A');
        }

        var counts = new int[26 * 26];
        if (cipherIdx.Length >= 2)
        {
            ref var cntBase = ref MemoryMarshal.GetArrayDataReference(counts);
            for (var i = 1; i < cipherIdx.Length; i++)
            {
                int r = cipherIdx[i - 1];
                int c = cipherIdx[i];
                Unsafe.Add(ref cntBase, r * 26 + c)++;
            }
        }

        var bestGlobalPerm = new char[26];
        var bestGlobalScore = double.NegativeInfinity;

        var rng = new Xoshiro256(((ulong)s.Length << 32) ^ (ulong)Environment.TickCount64);

        for (var restart = 0; restart < RestartCount; restart++)
        {
            var permArr = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
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

            var scurr = model.ScoreFromCounts(invPos, counts);
            var sbest = scurr;

            var bestLocalPerm = new char[26];
            perm.CopyTo(bestLocalPerm);

            var T = T0;

            for (var it = 0; it < IterationCount; it++)
            {
                var i = rng.NextInt(26);
                var j = rng.NextInt(25);
                if (j >= i)
                {
                    j++;
                }

                var snew = model.ProposedScoreDelta(invPos, perm, counts, i, j, scurr);
                var dS = snew - scurr;

                bool accept;
                if (dS >= 0d)
                {
                    accept = true;
                }
                else
                {
                    var u = (float)rng.NextDouble();
                    accept = MathF.Log(u) < (float)(dS / T);
                }

                if (accept)
                {
                    (perm[i], perm[j]) = (perm[j], perm[i]);
                    invPos[(byte)(perm[i] - 'A')] = (byte)i;
                    invPos[(byte)(perm[j] - 'A')] = (byte)j;
                    scurr = snew;

                    if (scurr > sbest)
                    {
                        sbest = scurr;
                        perm.CopyTo(bestLocalPerm);
                    }
                }

                T *= Alpha;
            }

            if (sbest > bestGlobalScore)
            {
                bestGlobalScore = sbest;
                bestLocalPerm.CopyTo(bestGlobalPerm, 0);
            }
        }

        var bestPermutation = new string(bestGlobalPerm);
        var bestPlainText = cipher.Decrypt(normalizedCipher, alphabet, bestPermutation);
        return new HeuristicResult(bestPermutation, bestPlainText, bestGlobalScore);
    }
}