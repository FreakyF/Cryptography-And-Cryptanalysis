using Task01.Application.Abstractions;

namespace Task01.Infrastructure.IO;

public sealed class FileWriter : IFileWriter
{
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