namespace Task04.Application.Abstractions;

public interface IFileWriter
{
    void WriteAll(string path, string content);
}