namespace Task02.Application.Abstractions;

public interface IKeyService
{
    /// <summary>Retrieves the encryption key from a file and converts it to an integer asynchronously.</summary>
    /// <param name="keyFilePath">The file path containing the key representation.</param>
    /// <returns>A task that resolves to the parsed integer key value.</returns>
    Task<int> GetKeyAsync(string keyFilePath);
}