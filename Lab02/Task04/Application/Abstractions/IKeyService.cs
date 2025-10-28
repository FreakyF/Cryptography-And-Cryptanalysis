namespace Task04.Application.Abstractions;

public interface IKeyService
{
    Task<(int A, int B)> GetKeyAsync(string keyFilePath);
}