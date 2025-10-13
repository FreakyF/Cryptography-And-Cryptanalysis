namespace Task04.Domain;

public static class Alphabet
{
    public const string LatinUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static bool IsUpperLatin(char c) => c is >= 'A' and <= 'Z';
}