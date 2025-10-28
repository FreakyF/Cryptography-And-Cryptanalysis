namespace Task03.Application.Abstractions;

public interface IFileService
{
    /// <summary>Asynchronously reads the entire contents of the specified file into a string.</summary>
    /// <param name="path">The path to the file that should be read.</param>
    /// <returns>The complete text retrieved from the file.</returns>
    Task<string> ReadAllTextAsync(string path);
    /// <summary>Asynchronously writes the supplied text into the specified file, replacing any existing content.</summary>
    /// <param name="path">The destination file path that should receive the text.</param>
    /// <param name="content">The textual content that will be persisted.</param>
    /// <returns>A task that completes when the write operation has finished.</returns>
    Task WriteAllTextAsync(string path, string content);
}