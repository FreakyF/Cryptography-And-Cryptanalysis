namespace Task03.Application.Abstractions;

public interface IKeyService
{
    /// <summary>Retrieves and parses the affine cipher key pair from the specified file.</summary>
    /// <param name="keyFilePath">The path to the key file containing the coefficients.</param>
    /// <returns>The validated multiplicative and additive key values.</returns>
    Task<(int A, int B)> GetKeyAsync(string keyFilePath);
}