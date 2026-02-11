using Task04.Domain.Models;

namespace Task04.Domain.Abstractions;

public interface IBigramScorer
{
    BigramWeights LoadWeights(string bigramsText, double alpha = 0.01);

    double Score(string normalizedText, BigramWeights weights);
}