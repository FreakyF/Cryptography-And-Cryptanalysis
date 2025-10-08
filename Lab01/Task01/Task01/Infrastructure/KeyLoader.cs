using Task01.Domain;

namespace Task01.Infrastructure;

public class KeyLoader : IKeyLoader
{
    public Dictionary<char, char> Load(string path)
    {
        EnsureFileExists(path);

        var map = new Dictionary<char, char>();

        foreach (var line in File.ReadAllLines(path))
        {
            var (from, to) = ParseLine(line);
            map[from] = to;
        }

        return map;
    }

    private static void EnsureFileExists(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"The key file was not found at the specified path (path: {path}). " +
                "Please provide a valid key file."
            );
        }
    }

    private static (char from, char to) ParseLine(string line)
    {
        var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        ValidateParts(parts, line);

        var from = char.ToUpperInvariant(parts[0][0]);
        var to = char.ToUpperInvariant(parts[1][0]);

        return (from, to);
    }

    private static void ValidateParts(string[] parts, string originalLine)
    {
        if (parts.Length != 2 || parts[0].Length != 1 || parts[1].Length != 1)
        {
            throw new FormatException(
                $"Invalid line in key file (line: {originalLine}). " +
                "Each line must contain exactly two single characters separated by whitespace."
            );
        }
    }
}