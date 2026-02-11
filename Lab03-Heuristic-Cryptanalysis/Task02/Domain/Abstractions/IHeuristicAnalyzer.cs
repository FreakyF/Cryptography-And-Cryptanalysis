using Task02.Domain.Models;

namespace Task02.Domain.Abstractions;

public interface IHeuristicAnalyzer
{
    /// <summary>
    ///     Attempts to recover the substitution permutation and plaintext by applying a Metropolis-Hastings
    ///     heuristic search over the space of possible keys.
    /// </summary>
    /// <param name="cipherText">The cipher text that will be analyzed; non-letter characters are ignored.</param>
    /// <param name="referenceText">The reference text used to build the bigram probability model.</param>
    /// <param name="alphabet">The ordered set of characters representing the plaintext alphabet.</param>
    /// <returns>The best heuristic result containing the recovered permutation and plaintext candidate.</returns>
    HeuristicResult Analyze(string cipherText, string referenceText, string alphabet);
}