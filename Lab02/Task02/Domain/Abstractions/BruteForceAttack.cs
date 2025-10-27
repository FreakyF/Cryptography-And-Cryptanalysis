using Task02.Domain.Models;

namespace Task02.Domain.Abstractions;

using MathNet.Numerics.Distributions;

public sealed class BruteForceAttack(ICaesarCipher cipher, IChiSquareScorer scorer) : IBruteForceAttack
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public BruteForceResult BreakCipher(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return new BruteForceResult(string.Empty, 0, double.PositiveInfinity, false);

        var bestPlain = string.Empty;
        var bestScore = double.PositiveInfinity;
        var bestKey = 0;

        for (var key = 0; key < 26; key++)
        {
            var candidate = cipher.Decrypt(cipherText, Alphabet, key);
            var score = scorer.Score(candidate);

            if (score >= bestScore)
            {
                continue;
            }

            bestScore = score;
            bestPlain = candidate;
            bestKey = key;
        }

        const double df = 25.0;
        const double p = 0.95;
        var critical = ChiSquared.InvCDF(df, p);

        var looksEnglish = bestScore <= critical;

        return new BruteForceResult(
            bestPlain,
            bestKey,
            bestScore,
            looksEnglish
        );
    }
}