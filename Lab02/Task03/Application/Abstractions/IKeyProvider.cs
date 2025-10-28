namespace Task03.Application.Abstractions;

public interface IKeyProvider
{
    Task<int> GetKeyAsync(string keyFilePath);
}