namespace Task02.Domain.Abstractions;

public interface IChiSquareScorer
{
    double Score(string text);
}