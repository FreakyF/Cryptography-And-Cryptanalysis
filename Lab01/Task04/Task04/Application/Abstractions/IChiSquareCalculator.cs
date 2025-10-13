using Task04.Application.Models;

namespace Task04.Application.Abstractions;

public interface IChiSquareCalculator
{
    double Compute(string normalizedText, int n, NGramReference reference, ChiSquareOptions? options = null);
}