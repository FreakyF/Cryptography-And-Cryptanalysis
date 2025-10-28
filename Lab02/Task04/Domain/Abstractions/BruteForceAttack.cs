using MathNet.Numerics.Distributions;
using Task04.Domain.Models;

namespace Task04.Domain.Abstractions;

public sealed class BruteForceAttack(IAffineCipher cipher, IChiSquareScorer scorer) : IBruteForceAttack
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly int[] InvertibleA = [1, 3, 5, 7, 9, 11, 15, 17, 19, 21, 23, 25];

    /// <summary>Searches all invertible affine keys and returns the best scoring plaintext candidate.</summary>
    /// <param name="cipherText">The ciphertext to be analyzed.</param>
    /// <returns>The brute-force result describing the candidate plaintext and metadata.</returns>
    public BruteForceResult BreakCipher(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return new BruteForceResult(string.Empty, 0, 0, double.PositiveInfinity, false);
        }

        var bestPlain = string.Empty;
        var bestScore = double.PositiveInfinity;
        int bestA = 0, bestB = 0;

        foreach (var a in InvertibleA)
        {
            for (var b = 0; b < 26; b++)
            {
                var cand = cipher.Decrypt(cipherText, Alphabet, a, b);
                var score = scorer.Score(cand);

                if (!(score < bestScore))
                {
                    continue;
                }

                bestScore = score;
                bestPlain = cand;
                bestA = a;
                bestB = b;
            }
        }

        var critical = ChiSquared.InvCDF(25.0, 0.95);
        var looksEnglish = bestScore <= critical;

        return new BruteForceResult(bestPlain, bestA, bestB, bestScore, looksEnglish);
    }
}