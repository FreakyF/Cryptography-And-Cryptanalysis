using Task02.Application.Abstractions;

namespace Task02.Infrastructure.IO;

public sealed class FileWriter : IFileWriter
{
    /// <summary>Writes the entire content to the specified file path using a temporary file to ensure atomic replacement.</summary>
    /// <param name="path">The destination file path where the content should be stored.</param>
    /// <param name="content">The full text content that will be written to the file.</param>
    public void WriteAll(string path, string content)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is null or empty.", nameof(path));

        var full = Path.GetFullPath(path);
        var dir = Path.GetDirectoryName(full) ?? ".";
        Directory.CreateDirectory(dir);

        var tmp = Path.Combine(dir, $"{Path.GetFileName(full)}.{Guid.NewGuid():N}.tmp");
        File.WriteAllText(tmp, content);
        File.Move(tmp, full, overwrite: true);
    }
}