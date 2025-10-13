namespace Task03.Application.Abstractions;

public interface IFileReader
{
    string ReadAll(string path);
}