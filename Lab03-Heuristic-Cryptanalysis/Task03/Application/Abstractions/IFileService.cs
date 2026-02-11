namespace Task03.Application.Abstractions;

public interface IFileService
{
    /// <summary>Reads the full contents of the file at the provided path using UTF-8 encoding.</summary>
    /// <param name="path">The absolute or relative file path to read.</param>
    /// <returns>The entire file contents as a single string.</returns>
    string ReadAllText(string path);

    /// <summary>Writes the supplied content to the specified path, replacing any existing file.</summary>
    /// <param name="path">The destination file path where the data should be persisted.</param>
    /// <param name="content">The text payload that will be written to disk.</param>
    void WriteAllText(string path, string content);
}