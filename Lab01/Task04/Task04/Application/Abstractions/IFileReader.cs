namespace Task04.Application.Abstractions;

public interface IFileReader
{
    string ReadAll(string path);
}