using Task04.Application.Abstractions;

namespace Task04.Infrastructure.IO;

public sealed class FileReader : IFileReader
{
    public string ReadAll(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? throw new ArgumentException("Path is null or empty.", nameof(path))
            : File.ReadAllText(path);
    }
}