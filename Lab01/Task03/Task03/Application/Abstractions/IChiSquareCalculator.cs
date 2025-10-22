namespace Task03.Application.Abstractions;

public interface IChiSquareCalculator
{
    /// <summary>Computes the chi-square statistic comparing observed n-gram counts to a reference distribution.</summary>
    /// <param name="normalizedText">The normalized text whose n-gram frequencies should be analyzed.</param>
    /// <param name="n">The n-gram order associated with the analysis.</param>
    /// <param name="reference">The reference probability distribution for the same n-gram order.</param>
    /// <returns>The chi-square test statistic quantifying deviation from the reference.</returns>
    double Compute(string normalizedText, int n, NGramReference reference);
}