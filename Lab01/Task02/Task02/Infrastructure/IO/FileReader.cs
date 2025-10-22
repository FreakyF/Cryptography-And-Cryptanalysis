using Task02.Application.Abstractions;

namespace Task02.Infrastructure.IO;

public sealed class FileReader : IFileReader
{
    /// <summary>Reads the entire file content from the provided path after validating the path value.</summary>
    /// <param name="path">The source file path from which all text should be read.</param>
    /// <returns>The complete text content read from the specified file.</returns>
    public string ReadAll(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? throw new ArgumentException("Path is null or empty.", nameof(path))
            : File.ReadAllText(path);
    }
}