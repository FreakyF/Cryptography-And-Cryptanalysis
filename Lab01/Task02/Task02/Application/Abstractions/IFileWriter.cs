namespace Task02.Application.Abstractions;

public interface IFileWriter
{
    void WriteAll(string path, string content);
}