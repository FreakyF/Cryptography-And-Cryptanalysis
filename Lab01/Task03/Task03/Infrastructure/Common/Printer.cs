namespace Task02.Infrastructure.Common;

internal static class Printer
{
    public static void Errors(IEnumerable<string> errors)
    {
        foreach (var e in errors)
            Console.Error.WriteLine($"ERROR: {e}");
    }

    public static void Usage()
    {
        Console.WriteLine("""
                          Usage:
                            # cipher
                            dotnet run -- -i <input> -o <output> -k <key> (-e | -d)

                            # n-gram generation
                            dotnet run -- -i <input> [-g1 <mono_out>] [-g2 <bi_out>] [-g3 <tri_out>] [-g4 <quad_out>]

                            # chi-square test against reference base
                            dotnet run -- -i <input> -s (-r1 <mono_ref> | -r2 <bi_ref> | -r3 <tri_ref> | -r4 <quad_ref>)

                          Flags:
                            -i, --input     Input text file.
                            -o, --output    Output file (cipher).
                            -k, --key       Substitution key file (cipher).
                            -e, --encrypt   Encrypt.
                            -d, --decrypt   Decrypt.
                            -g1             Output file for monograms.
                            -g2             Output file for bigrams.
                            -g3             Output file for trigrams.
                            -g4             Output file for quadgrams.
                            -r1..-r4        Reference base file for n-grams of order 1..4.
                            -s, --chisq     Compute chi-square statistic and print to stdout.
                            -h, --help      Show help.

                          Examples:
                            # cipher
                            dotnet run -- -e -i plaintext.txt -o cipher.txt -k key.txt
                            dotnet run -- -d -i cipher.txt   -o plain.txt  -k key.txt

                            # n-grams
                            dotnet run -- -i plaintext.txt -g1 mono.txt
                            dotnet run -- -i plaintext.txt -g2 bi.txt -g3 tri.txt -g4 quad.txt

                            # chi-square
                            dotnet run -- -i plaintext.txt -s -r1 mono_ref.txt
                            dotnet run -- -i plaintext.txt -s -r2 bi_ref.txt
                            dotnet run -- -i plaintext.txt -s -r3 tri_ref.txt
                            dotnet run -- -i plaintext.txt -s -r4 quad_ref.txt
                          """);
    }
}