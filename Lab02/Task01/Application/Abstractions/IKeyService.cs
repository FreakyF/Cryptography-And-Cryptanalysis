namespace Task01.Application.Abstractions;

public interface IKeyService
{
    Task<int> GetKeyAsync(string keyFilePath);
}