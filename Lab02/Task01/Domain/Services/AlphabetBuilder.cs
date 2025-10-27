using Task01.Domain.Abstractions;

namespace Task01.Domain.Services;

public sealed class AlphabetBuilder : IAlphabetBuilder
{
    public string BuildAlphabet(string normalizedText)
    {
        if (string.IsNullOrEmpty(normalizedText))
        {
            return string.Empty;
        }

        var set = new HashSet<char>();

        foreach (var c in normalizedText)
        {
            set.Add(c);
        }

        if (set.Count == 0)
        {
            return string.Empty;
        }

        var arr = set.ToArray();
        Array.Sort(arr);

        return new string(arr);
    }
}