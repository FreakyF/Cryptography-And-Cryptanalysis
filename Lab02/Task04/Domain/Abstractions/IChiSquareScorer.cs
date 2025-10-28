namespace Task04.Domain.Abstractions;

public interface IChiSquareScorer
{
    double Score(string text);
}