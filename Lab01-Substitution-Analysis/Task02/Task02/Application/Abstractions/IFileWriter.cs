namespace Task02.Application.Abstractions;

public interface IFileWriter
{
    /// <summary>Writes the specified content to the file at the provided path, overwriting existing data.</summary>
    /// <param name="path">The file path that should receive the content.</param>
    /// <param name="content">The text that will be written to the target file.</param>
    void WriteAll(string path, string content);
}