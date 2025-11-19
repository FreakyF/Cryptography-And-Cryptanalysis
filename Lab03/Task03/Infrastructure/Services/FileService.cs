using Task03.Application.Abstractions;

namespace Task03.Infrastructure.Services;

public sealed class FileService : IFileService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    public string ReadAllText(string path)
    {
        return File.ReadAllText(path, Utf8NoBom);
    }

    public void WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content, Utf8NoBom);
    }
}