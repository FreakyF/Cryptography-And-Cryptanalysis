namespace Task01.Application.Abstractions;

public interface IFileReader
{
    /// <summary>Reads and returns the entire content of the file located at the specified path.</summary>
    /// <param name="path">The filesystem path to the file that should be read.</param>
    /// <returns>The full textual content extracted from the referenced file.</returns>
    string ReadAll(string path);
}