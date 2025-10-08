using CommandLine;
using CommandLine.Text;
using Task01.Domain;

namespace Task01.Application;

public class ConfigurationOptionsParser : IOptionsParser
{
    public CommandLineOptions Parse(string[] args)
    {
        var parser = new Parser(with => with.HelpWriter = null);
        var parserResult = parser.ParseArguments<ParserOptions>(args);

        return parserResult.MapResult(
            ParseValidOptions,
            _ =>
            {
                var helpText = HelpText.AutoBuild(parserResult, h =>
                {
                    h.Heading = string.Empty;
                    h.Copyright = string.Empty;
                    return h;
                }, e => e);

                var lines = helpText.ToString()
                    .Split(Environment.NewLine);

                var core = lines
                    .SkipWhile(string.IsNullOrWhiteSpace)
                    .Reverse()
                    .SkipWhile(string.IsNullOrWhiteSpace)
                    .Reverse();

                var cleaned = string.Join(Environment.NewLine,
                    new[] { string.Empty }
                        .Concat(core)
                        .Concat([string.Empty]));

                Console.WriteLine(cleaned);
                Environment.Exit(1);


                return null!;
            });
    }

    private static CommandLineOptions ParseValidOptions(ParserOptions options)
    {
        ValidateMode(options);
        return BuildCommandLineOptions(options);
    }

    private static void ValidateMode(ParserOptions options)
    {
        if (options.Encrypt == options.Decrypt)
        {
            throw new ArgumentException(
                "Invalid mode selection. You must specify exactly one mode."
            );
        }
    }

    private static CommandLineOptions BuildCommandLineOptions(ParserOptions options)
    {
        return new CommandLineOptions
        {
            InputFile = options.Input!,
            OutputFile = options.Output!,
            KeyFile = options.Key!,
            Mode = options.Encrypt ? CipherMode.Encrypt : CipherMode.Decrypt
        };
    }
}