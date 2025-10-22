namespace Task04.Application.Abstractions;

public interface ITextNormalizer
{
    /// <summary>Normalizes raw text by uppercasing letters and stripping non-alphabetic characters.</summary>
    /// <param name="input">The raw text to normalize.</param>
    /// <returns>The normalized, uppercase-only string.</returns>
    string Normalize(string input);
}