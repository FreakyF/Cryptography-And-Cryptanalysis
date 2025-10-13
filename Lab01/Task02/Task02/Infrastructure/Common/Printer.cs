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
                            dotnet run -- -i <input_file> -o <output_file> -k <key_file> (-e | -d)

                          Flags:
                            -i, --input     Path to the plaintext/ciphertext file.
                            -o, --output    Path to the output file.
                            -k, --key       Path to the substitution table file.
                            -e, --encrypt   Encryption mode.
                            -d, --decrypt   Decryption mode.

                          Examples:
                            dotnet run -- -e -i input.txt -o out.txt -k key.txt
                            dotnet run -- -d -k key.txt -o plain.txt -i cipher.txt
                            
                          """);
    }
}