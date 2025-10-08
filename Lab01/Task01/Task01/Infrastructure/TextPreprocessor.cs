using Task01.Domain;

namespace Task01.Infrastructure;

public class TextPreprocessor : ITextPreprocessor
{
    public string Clean(string input) => new(input.ToUpperInvariant().Where(char.IsLetter).ToArray());
}