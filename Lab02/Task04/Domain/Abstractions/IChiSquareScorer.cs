namespace Task04.Domain.Abstractions;

public interface IChiSquareScorer
{
    /// <summary>Computes the chi-square statistic for the supplied text against English letter frequencies.</summary>
    /// <param name="text">The uppercase text whose distribution should be evaluated.</param>
    /// <returns>The chi-square score, where smaller values indicate a closer match to English.</returns>
    double Score(string text);
}