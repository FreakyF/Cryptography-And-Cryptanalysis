namespace Task01.Application.Abstractions;

public interface IFileReader
{
    string ReadAll(string path);
}