namespace Task04.Application.Abstractions;

public interface INGramCounter
{
    /// <summary>Counts occurrences of every n-gram within the supplied normalized text.</summary>
    /// <param name="normalized">The uppercase alphabetic text to analyze.</param>
    /// <param name="n">The n-gram size to count.</param>
    /// <returns>A dictionary mapping each n-gram to its frequency.</returns>
    IReadOnlyDictionary<string, int> Count(string normalized, int n);
}