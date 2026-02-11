namespace Task04.Application.Abstractions;

public interface IFileWriter
{
    /// <summary>Writes the provided content to the specified path, overwriting any existing file.</summary>
    /// <param name="path">The destination path for the written file.</param>
    /// <param name="content">The text content to persist.</param>
    void WriteAll(string path, string content);
}