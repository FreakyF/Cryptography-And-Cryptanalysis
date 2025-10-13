namespace Task02.Application.Abstractions;

public interface IFileReader
{
    string ReadAll(string path);
}