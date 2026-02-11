namespace Task03.Domain.Abstractions;

public interface ITextNormalizer
{
    /// <summary>Transforms raw input into an uppercase alphabet-only string suitable for cipher processing.</summary>
    /// <param name="input">The text to normalize prior to encryption or decryption.</param>
    /// <returns>The normalized string containing only uppercase alphabetic characters.</returns>
    string Normalize(string input);
}