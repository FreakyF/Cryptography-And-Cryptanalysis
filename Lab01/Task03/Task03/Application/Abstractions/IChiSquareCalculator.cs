namespace Task03.Application.Abstractions;

public interface IChiSquareCalculator
{
    double Compute(string normalizedText, int n, NGramReference reference);
}