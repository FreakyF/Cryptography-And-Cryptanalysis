namespace Task01.Application.Abstractions;

public interface IFileService
{
    /// <summary>Reads the entire contents of a file asynchronously into a string.</summary>
    /// <param name="path">The path of the file to read.</param>
    /// <returns>A task that yields the full text contained in the file.</returns>
    Task<string> ReadAllTextAsync(string path);

    /// <summary>Writes the provided text to a file asynchronously, replacing any existing content.</summary>
    /// <param name="path">The path of the file to write.</param>
    /// <param name="content">The text content that should be stored in the file.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task WriteAllTextAsync(string path, string content);
}