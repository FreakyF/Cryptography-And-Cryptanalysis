using Task02.Domain.Abstractions;
using Task02.Domain.Models;

namespace Task02.Domain.Services;

public sealed class MetropolisHastingsAnalyzer(
    ITextNormalizer textNormalizer,
    ISubstitutionCipher cipher)
    : IHeuristicAnalyzer
{
    private const double SmoothingConstant = 0.01d;
    private const int IterationCount = 500000;

    /// <summary>
    ///     Performs a Metropolis-Hastings search to approximate the substitution permutation that maximizes the
    ///     likelihood of the decrypted text under the supplied language model.
    /// </summary>
    /// <param name="cipherText">The cipher text that requires decryption; non-letter characters are ignored.</param>
    /// <param name="referenceText">The text used to build the reference bigram language model.</param>
    /// <param name="alphabet">The alphabet used by the substitution cipher.</param>
    /// <returns>The heuristic result with the most likely permutation and plaintext candidate.</returns>
    public HeuristicResult Analyze(string cipherText, string referenceText, string alphabet)
    {
        if (string.IsNullOrEmpty(alphabet))
        {
            throw new ArgumentException("Alphabet must be provided", nameof(alphabet));
        }

        var normalizedCipher = textNormalizer.Normalize(cipherText);
        if (normalizedCipher.Length == 0)
        {
            return new HeuristicResult(alphabet, string.Empty, double.NegativeInfinity);
        }

        var normalizedReference = textNormalizer.Normalize(referenceText);
        var model = BigramLanguageModel.Create(alphabet, normalizedReference, SmoothingConstant);

        var currentPermutation = alphabet;
        var currentPlain = cipher.Decrypt(normalizedCipher, alphabet, currentPermutation);
        var currentScore = model.Score(currentPlain);

        var bestPermutation = currentPermutation;
        var bestPlainText = currentPlain;
        var bestScore = currentScore;

        for (var iteration = 0; iteration < IterationCount; iteration++)
        {
            var proposalPermutation = ProposePermutation(currentPermutation);
            var proposalPlain = cipher.Decrypt(normalizedCipher, alphabet, proposalPermutation);
            var proposalScore = model.Score(proposalPlain);
            var acceptanceProbability = ComputeAcceptanceProbability(currentScore, proposalScore);

            if (Random.Shared.NextDouble() > acceptanceProbability)
            {
                continue;
            }

            currentPermutation = proposalPermutation;
            currentScore = proposalScore;

            if (proposalScore <= bestScore)
            {
                continue;
            }

            bestScore = proposalScore;
            bestPermutation = proposalPermutation;
            bestPlainText = proposalPlain;
        }

        return new HeuristicResult(bestPermutation, bestPlainText, bestScore);
    }

    /// <summary>Proposes a new permutation by swapping two randomly selected positions.</summary>
    /// <param name="permutation">The current permutation.</param>
    /// <returns>A new permutation string with two positions swapped.</returns>
    private static string ProposePermutation(string permutation)
    {
        var chars = permutation.ToCharArray();
        var firstIndex = Random.Shared.Next(chars.Length);
        var secondIndex = Random.Shared.Next(chars.Length - 1);
        if (secondIndex >= firstIndex)
        {
            secondIndex++;
        }

        (chars[firstIndex], chars[secondIndex]) = (chars[secondIndex], chars[firstIndex]);
        return new string(chars);
    }

    /// <summary>Calculates the Metropolis-Hastings acceptance probability for the proposed permutation.</summary>
    /// <param name="currentScore">The log-likelihood of the current plaintext candidate.</param>
    /// <param name="proposalScore">The log-likelihood of the proposed plaintext candidate.</param>
    /// <returns>The acceptance probability bounded to the [0, 1] range.</returns>
    private static double ComputeAcceptanceProbability(double currentScore, double proposalScore)
    {
        if (proposalScore >= currentScore)
        {
            return 1d;
        }

        var ratio = Math.Exp(proposalScore - currentScore);
        return ratio >= 1d ? 1d : ratio;
    }

    private sealed class BigramLanguageModel
    {
        private readonly Dictionary<char, int> _indices;
        private readonly double[,] _logProbabilities;

        private BigramLanguageModel(Dictionary<char, int> indices, double[,] logProbabilities)
        {
            _indices = indices;
            _logProbabilities = logProbabilities;
        }

        /// <summary>Creates a bigram language model with additive smoothing applied to the counts.</summary>
        /// <param name="alphabet">The alphabet used to build the matrix indices.</param>
        /// <param name="referenceText">The normalized reference text providing the bigram statistics.</param>
        /// <param name="alpha">The additive smoothing constant.</param>
        /// <returns>The constructed language model ready to score plaintext candidates.</returns>
        public static BigramLanguageModel Create(string alphabet, string referenceText, double alpha)
        {
            var index = new Dictionary<char, int>(alphabet.Length);
            for (var i = 0; i < alphabet.Length; i++)
            {
                index[alphabet[i]] = i;
            }

            var size = alphabet.Length;
            var counts = new double[size, size];

            if (!string.IsNullOrEmpty(referenceText))
            {
                var span = referenceText.AsSpan();
                for (var i = 0; i < span.Length - 1; i++)
                {
                    if (!index.TryGetValue(span[i], out var row))
                    {
                        continue;
                    }

                    if (!index.TryGetValue(span[i + 1], out var column))
                    {
                        continue;
                    }

                    counts[row, column] += 1d;
                }
            }

            var total = 0d;
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    counts[i, j] += alpha;
                    total += counts[i, j];
                }
            }

            var logProbs = new double[size, size];
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    logProbs[i, j] = Math.Log(counts[i, j] / total);
                }
            }

            return new BigramLanguageModel(index, logProbs);
        }

        /// <summary>Calculates the log-likelihood score for the provided plaintext under the model.</summary>
        /// <param name="plainText">The plaintext candidate to evaluate.</param>
        /// <returns>The summed log probability of all bigrams in the plaintext.</returns>
        public double Score(string plainText)
        {
            if (string.IsNullOrEmpty(plainText) || plainText.Length < 2)
            {
                return 0d;
            }

            var span = plainText.AsSpan();
            var score = 0d;

            for (var i = 0; i < span.Length - 1; i++)
            {
                if (!_indices.TryGetValue(span[i], out var row))
                {
                    continue;
                }

                if (!_indices.TryGetValue(span[i + 1], out var column))
                {
                    continue;
                }

                score += _logProbabilities[row, column];
            }

            return score;
        }
    }
}