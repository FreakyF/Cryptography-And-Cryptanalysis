using Task04.Domain;

namespace Task04.Application.Abstractions;

public interface IKeyLoader
{
    /// <summary>Loads a substitution key definition from the specified file path.</summary>
    /// <param name="path">The path to the key file to read.</param>
    /// <returns>A parsed substitution key containing forward and reverse mappings.</returns>
    SubstitutionKey Load(string path);
}