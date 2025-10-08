using Task01.Application;
using Task01.Domain;
using Task01.Infrastructure;

namespace Task01;

#pragma warning disable CA1859
internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            IOptionsParser parser = new ConfigurationOptionsParser();
            
            var options = parser.Parse(args);

            IKeyLoader keyLoader = new KeyLoader();
            var keyMap = keyLoader.Load(options.KeyFile);

            ICipher cipher = new SubstitutionCipher(keyMap);
            ITextPreprocessor preprocessor = new TextPreprocessor();

            var inputText = File.ReadAllText(options.InputFile);
            var cleanedText = preprocessor.Clean(inputText);

            var result = options.Mode switch
            {
                CipherMode.Encrypt => cipher.Encrypt(cleanedText),
                CipherMode.Decrypt => cipher.Decrypt(cleanedText),
                _ => throw new InvalidOperationException("Unknown type")
            };

            File.WriteAllText(options.OutputFile, result);

            Console.WriteLine($"Task ended. Results are saved in {options.OutputFile}");
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return 1;
        }
    }
}