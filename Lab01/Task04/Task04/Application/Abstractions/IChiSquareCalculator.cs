using Task04.Application.Models;

namespace Task04.Application.Abstractions;

public interface IChiSquareCalculator
{
    /// <summary>Computes the chi-square statistic for the provided normalized text using the supplied reference distribution.</summary>
    /// <param name="normalizedText">The uppercase alphabetic text to evaluate.</param>
    /// <param name="n">The n-gram order to analyze.</param>
    /// <param name="reference">The reference probabilities against which to compare.</param>
    /// <param name="options">Optional configuration such as exclusions or minimum expected counts.</param>
    /// <returns>The chi-square statistic representing divergence from the reference distribution.</returns>
    double Compute(string normalizedText, int n, NGramReference reference, ChiSquareOptions? options = null);
}