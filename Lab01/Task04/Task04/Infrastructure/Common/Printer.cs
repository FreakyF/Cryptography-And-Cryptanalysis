namespace Task04.Infrastructure.Common;

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

                            # n-gram generation (counts)
                            dotnet run -- -i <input> [-g1 <mono_out>] [-g2 <bi_out>] [-g3 <tri_out>] [-g4 <quad_out>]

                            # build reference from corpus (probabilities)
                            dotnet run -- -i <corpus> [-b1 <mono_ref>] [-b2 <bi_ref>] [-b3 <tri_ref>] [-b4 <quad_ref>]

                            # chi-square test against reference
                            dotnet run -- -i <input> -s -r1 <mono_ref> | -r2 <bi_ref> | -r3 <tri_ref> | -r4 <quad_ref>

                            # chi-square experiments
                            dotnet run -- -i <input> -s -r1 <mono_ref> -nlen 1000
                            dotnet run -- -i <input> -s -r1 <mono_ref> --exclude J,K,Q,X,Z
                            dotnet run -- -i <input> -s -r3 <tri_ref> --minE 5

                          Flags:
                            -i, --input     Input text file.
                            -o, --output    Output file (cipher).
                            -k, --key       Substitution key file (cipher).
                            -e, --encrypt   Encrypt.
                            -d, --decrypt   Decrypt.
                            -g1..-g4        Output files for n-gram COUNT tables.
                            -b1..-b4        Output files for reference PROBABILITY tables.
                            -r1..-r4        Reference file for chi-square of order 1..4.
                            -s, --chisq     Compute chi-square statistic and print to stdout.
                            -nlen           Limit normalized input length for chi-square.
                            --exclude       Comma-separated n-grams to exclude (e.g., J,K,Q,X,Z).
                            --minE          Skip classes with expected count < value.
                            -h, --help      Show this help.

                          Examples:
                            # cipher
                            dotnet run -- -e -i plaintext.txt -o cipher.txt -k key.txt
                            dotnet run -- -d -i cipher.txt   -o plain.txt  -k key.txt

                            # n-gram counts
                            dotnet run -- -i text.txt -g2 bigrams.txt
                            dotnet run -- -i text.txt -g1 mono.txt -g2 bi.txt -g3 tri.txt -g4 quad.txt

                            # build reference from corpus
                            dotnet run -- -i corpus.txt -b2 bi_ref.txt
                            dotnet run -- -i corpus.txt -b1 mono_ref.txt -b4 quad_ref.txt

                            # chi-square test
                            dotnet run -- -i analyzed.txt -s -r2 bi_ref.txt

                            # chi-square experiments
                            dotnet run -- -i analyzed.txt -s -r1 mono_ref.txt -nlen 1000
                            dotnet run -- -i analyzed.txt -s -r1 mono_ref.txt --exclude J,K,Q,X,Z
                            dotnet run -- -i analyzed.txt -s -r3 tri_ref.txt  --minE 5
                            
                          """);
    }
}