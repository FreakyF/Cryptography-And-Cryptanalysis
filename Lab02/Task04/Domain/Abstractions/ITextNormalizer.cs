namespace Task04.Domain.Abstractions;

public interface ITextNormalizer
{
    /// <summary>Normalizes raw text by stripping non-letters and converting the result to uppercase.</summary>
    /// <param name="input">The raw user-provided text.</param>
    /// <returns>The normalized uppercase text containing only alphabetic characters.</returns>
    string Normalize(string input);
}