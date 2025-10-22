namespace Task04.Application.Abstractions;

public interface IReferenceLoader
{
    /// <summary>Loads n-gram probabilities from the specified path into a reference distribution.</summary>
    /// <param name="path">The reference file containing n-gram probabilities.</param>
    /// <returns>A reference distribution describing the corpus probabilities.</returns>
    NGramReference Load(string path);
}