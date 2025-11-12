namespace Task02.Domain.Abstractions;

public interface ITextNormalizer
{
    /// <summary>Normalizes text by keeping alphabetic characters and converting them to uppercase.</summary>
    /// <param name="input">The raw text that may contain mixed characters and casing.</param>
    /// <returns>The sanitized uppercase string composed only of alphabetic characters.</returns>
    string Normalize(string input);
}