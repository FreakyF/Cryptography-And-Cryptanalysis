namespace Task01.Domain;

public static class Alphabet
{
    public const string LatinUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>Determines whether the specified character is an uppercase Latin letter between A and Z.</summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if the character is uppercase Latin; otherwise <see langword="false"/>.</returns>
    public static bool IsUpperLatin(char c) => c is >= 'A' and <= 'Z';
}