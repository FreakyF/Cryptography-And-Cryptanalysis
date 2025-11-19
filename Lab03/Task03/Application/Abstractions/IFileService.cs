namespace Task03.Application.Abstractions;

public interface IFileService
{
    string ReadAllText(string path);
    void WriteAllText(string path, string content);
}