namespace Task03.Application.Abstractions;

public interface INGramCounter
{
    /// <summary>Counts the occurrences of every n-gram in the provided normalized text.</summary>
    /// <param name="normalized">The uppercase text to analyze for n-gram frequencies.</param>
    /// <param name="n">The size of the n-grams that should be constructed.</param>
    /// <returns>A dictionary mapping each discovered n-gram to its number of occurrences.</returns>
    IReadOnlyDictionary<string, int> Count(string normalized, int n);
}