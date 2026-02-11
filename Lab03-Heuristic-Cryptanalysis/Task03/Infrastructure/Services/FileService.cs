using Task03.Application.Abstractions;

namespace Task03.Infrastructure.Services;

public sealed class FileService : IFileService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    /// <summary>Reads text from the specified path using UTF-8 encoding without emitting a BOM.</summary>
    /// <param name="path">The file path to read.</param>
    /// <returns>The full file contents as a string.</returns>
    public string ReadAllText(string path)
    {
        return File.ReadAllText(path, Utf8NoBom);
    }

    /// <summary>Writes the supplied content to the specified path using UTF-8 encoding without a BOM.</summary>
    /// <param name="path">The target file path to create or overwrite.</param>
    /// <param name="content">The string that will be persisted to the file.</param>
    public void WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content, Utf8NoBom);
    }
}