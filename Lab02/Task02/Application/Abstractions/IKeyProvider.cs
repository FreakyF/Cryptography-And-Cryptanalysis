namespace Task02.Application.Abstractions;

public interface IKeyProvider
{
    Task<int> GetKeyAsync(string keyFilePath);
}