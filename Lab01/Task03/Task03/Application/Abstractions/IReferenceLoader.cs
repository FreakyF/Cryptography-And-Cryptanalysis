namespace Task03.Application.Abstractions;

public interface IReferenceLoader
{
    /// <summary>Loads an n-gram probability reference distribution from the specified file.</summary>
    /// <param name="path">The file path pointing to the serialized reference data.</param>
    /// <returns>An n-gram reference describing order and probabilities.</returns>
    NGramReference Load(string path);
}