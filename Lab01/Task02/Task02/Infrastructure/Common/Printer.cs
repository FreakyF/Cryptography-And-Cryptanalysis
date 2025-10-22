namespace Task02.Infrastructure.Common;

internal static class Printer
{
    /// <summary>Writes each validation error message to the standard error output.</summary>
    /// <param name="errors">The collection of error messages to be printed.</param>
    public static void Errors(IEnumerable<string> errors)
    {
        foreach (var e in errors)
            Console.Error.WriteLine($"ERROR: {e}");
    }

    /// <summary>Displays the command line usage information for cipher and n-gram modes.</summary>
    public static void Usage()
    {
        Console.WriteLine("""

                          Usage:
                            # cipher
                            dotnet run -- -i <input_file> -o <output_file> -k <key_file> (-e | -d)

                            # n-grams
                            dotnet run -- -i <input_file> [-g1 <mono_out>] [-g2 <bi_out>] [-g3 <tri_out>] [-g4 <quad_out>]

                          Flags:
                            -i, --input     Path to input text file.
                            -o, --output    Path to output file (cipher mode).
                            -k, --key       Path to substitution key file (cipher mode).
                            -e, --encrypt   Encryption mode.
                            -d, --decrypt   Decryption mode.
                            -g1             Output file for monograms.
                            -g2             Output file for bigrams.
                            -g3             Output file for trigrams.
                            -g4             Output file for quadgrams.
                            -h, --help      Show this help.

                          Examples:
                            dotnet run -- -e -i plaintext.txt -o cipher.txt -k key.txt
                            dotnet run -- -d -i cipher.txt   -o plain.txt  -k key.txt
                            dotnet run -- -i plaintext.txt -g2 bigrams.txt
                            dotnet run -- -i plaintext.txt -g1 mono.txt -g2 bi.txt -g3 tri.txt -g4 quad.txt
                            
                          """);
    }
}