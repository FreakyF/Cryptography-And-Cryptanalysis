using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Task03.Domain.Abstractions;
using Task03.Domain.Models;

namespace Task03.Domain.Services;

public sealed class SimulatedAnnealingAnalyzer(
    ITextNormalizer textNormalizer,
    ISubstitutionCipher cipher)
    : IHeuristicAnalyzer, IConfigurableIterations
{
    private const int RestartCount = 8;
    private const double T0 = 5.0;
    private const double Alpha = 0.9995;
    private const double Smoothing = 0.01;

    private int _iterationCount = 500_000;

    public void SetIterations(int iterations)
    {
        _iterationCount = iterations > 0 ? iterations : 500_000;
    }

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

        var model = BigramLanguageModel.CreateFromBigramsText(referenceText, Smoothing);

        var s = normalizedCipher.AsSpan();
        var cipherIdx = new byte[s.Length];
        for (int i = 0; i < s.Length; i++)
            cipherIdx[i] = (byte)(s[i] - 'A');

        var counts = new int[26 * 26];
        if (cipherIdx.Length >= 2)
        {
            ref int cntBase = ref MemoryMarshal.GetArrayDataReference(counts);
            for (int i = 1; i < cipherIdx.Length; i++)
            {
                int r = cipherIdx[i - 1];
                int c = cipherIdx[i];
                Unsafe.Add(ref cntBase, r * 26 + c)++;
            }
        }

        var bestGlobalPerm = new char[26];
        double bestGlobalScore = double.NegativeInfinity;

        var rng = new Xoshiro256(((ulong)s.Length << 32) ^ (ulong)Environment.TickCount64);

        Span<byte> invPos = stackalloc byte[26];

        for (int restart = 0; restart < RestartCount; restart++)
        {
            var permArr = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (int i = permArr.Length - 1; i > 0; i--)
            {
                int j = rng.NextInt(i + 1);
                (permArr[i], permArr[j]) = (permArr[j], permArr[i]);
            }
            Span<char> perm = permArr.AsSpan();

            for (int i = 0; i < 26; i++)
                invPos[(byte)(perm[i] - 'A')] = (byte)i;

            double scurr = model.ScoreFromCounts(invPos, counts);
            double sbest = scurr;

            var bestLocalPerm = new char[26];
            perm.CopyTo(bestLocalPerm);

            double T = T0;

            for (int it = 0; it < _iterationCount; it++)
            {
                int i = rng.NextInt(26);
                int j = rng.NextInt(25);
                if (j >= i) j++;

                double snew = model.ProposedScoreDelta(invPos, perm, counts, i, j, scurr);
                double dS = snew - scurr;

                bool accept;
                if (dS >= 0d)
                {
                    accept = true;
                }
                else
                {
                    float u = (float)rng.NextDouble();
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

            if (!(sbest > bestGlobalScore))
            {
                continue;
            }

            bestGlobalScore = sbest;
            bestLocalPerm.CopyTo(bestGlobalPerm, 0);
        }

        var bestPermutation = new string(bestGlobalPerm);
        var bestPlainText = cipher.Decrypt(normalizedCipher, alphabet, bestPermutation);
        return new HeuristicResult(bestPermutation, bestPlainText, bestGlobalScore);
    }
}
