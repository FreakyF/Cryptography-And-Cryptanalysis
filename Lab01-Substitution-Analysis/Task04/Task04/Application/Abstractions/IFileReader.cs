namespace Task04.Application.Abstractions;

public interface IFileReader
{
    /// <summary>Reads the entire contents of the file located at the specified path.</summary>
    /// <param name="path">The location of the file to read.</param>
    /// <returns>The full text content of the file.</returns>
    string ReadAll(string path);
}