namespace Task04.Application.Abstractions;

public interface IKeyService
{
    /// <summary>Loads and validates the affine cipher key components from the provided file.</summary>
    /// <param name="keyFilePath">The path to the key file that should be read.</param>
    /// <returns>The multiplicative and additive key values extracted from the file.</returns>
    Task<(int A, int B)> GetKeyAsync(string keyFilePath);
}