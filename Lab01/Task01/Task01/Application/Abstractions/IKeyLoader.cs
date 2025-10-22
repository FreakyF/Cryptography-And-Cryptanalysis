using Task01.Domain;

namespace Task01.Application.Abstractions;

public interface IKeyLoader
{
    /// <summary>Loads and parses a substitution key definition from the specified file path.</summary>
    /// <param name="path">The path to the key file containing character mappings.</param>
    /// <returns>A substitution key constructed from the file contents.</returns>
    SubstitutionKey Load(string path);
}