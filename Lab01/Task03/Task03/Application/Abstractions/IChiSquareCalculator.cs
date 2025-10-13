namespace Task02.Application.Abstractions;

public interface IChiSquareCalculator
{
    double Compute(string normalizedText, int n, NGramReference reference);
}