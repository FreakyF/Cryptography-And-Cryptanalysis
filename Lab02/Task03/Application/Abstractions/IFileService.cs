namespace Task03.Application.Abstractions;

public interface IFileService
{
    Task<string> ReadAllTextAsync(string path);
    Task WriteAllTextAsync(string path, string content);
}