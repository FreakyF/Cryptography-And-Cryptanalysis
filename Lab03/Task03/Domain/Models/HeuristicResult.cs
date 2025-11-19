namespace Task03.Domain.Models;

/// <summary>Represents the outcome of a heuristic key search over the substitution cipher space.</summary>
/// <param name="Permutation">The recovered permutation that maps plaintext characters to cipher characters.</param>
/// <param name="PlainText">The plaintext obtained by decrypting the cipher text with the recovered permutation.</param>
/// <param name="LogLikelihood">The log-likelihood score of the plaintext under the reference language model.</param>
public sealed record HeuristicResult(string Permutation, string PlainText, double LogLikelihood);