namespace Task02.Application.Abstractions;

public interface IFileService
{
    /// <summary>Reads an entire text file into memory using UTF-8 input and returns its contents.</summary>
    /// <param name="path">The absolute or relative path to the file that should be read.</param>
    /// <returns>The full textual contents of the requested file.</returns>
    string ReadAllText(string path);

    /// <summary>Writes the provided text to the target file path using UTF-8 output without a BOM.</summary>
    /// <param name="path">The destination path where the content should be saved.</param>
    /// <param name="content">The textual payload that will be persisted.</param>
    void WriteAllText(string path, string content);
}