using Task02.Application.Abstractions;

namespace Task02.Infrastructure.Services;

public sealed class FileService : IFileService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    /// <summary>Reads the full contents of the specified file using UTF-8 without BOM and returns the resulting string.</summary>
    /// <param name="path">The path to the file that should be loaded.</param>
    /// <returns>The text contained in the requested file.</returns>
    public string ReadAllText(string path)
    {
        return File.ReadAllText(path, Utf8NoBom);
    }

    /// <summary>Writes the provided content to the specified file path using UTF-8 without BOM encoding.</summary>
    /// <param name="path">The destination file path to overwrite or create.</param>
    /// <param name="content">The textual payload to be written.</param>
    public void WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content, Utf8NoBom);
    }
}