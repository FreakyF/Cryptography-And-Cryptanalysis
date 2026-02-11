namespace Task02.Domain.Abstractions;

public interface IChiSquareScorer
{
    /// <summary>Calculates the chi-square statistic comparing the text's letter distribution to expected English frequencies.</summary>
    /// <param name="text">The normalized uppercase text to evaluate.</param>
    /// <returns>The chi-square score where lower values indicate closer matches to English.</returns>
    double Score(string text);
}