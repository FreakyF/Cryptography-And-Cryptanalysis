namespace Task02.Application.Abstractions;

public interface IKeyService
{
    Task<int> GetKeyAsync(string keyFilePath);
}