namespace Task01.Application.Abstractions;

public interface ITextNormalizer
{
    /// <summary>Produces an uppercase alphabetic string by filtering and transforming the provided input.</summary>
    /// <param name="input">The original text that requires normalization before encryption or decryption.</param>
    /// <returns>The normalized text containing only uppercase Latin letters.</returns>
    string Normalize(string input);
}