using Task01.Application;
using Task01.Application.Abstractions;
using Task01.Application.Cipher;
using Task01.Application.Text;
using Task01.Application.Validation;
using Task01.Infrastructure.CLI;
using Task01.Infrastructure.Common;
using Task01.Infrastructure.IO;
using Task01.Infrastructure.Validation;

namespace Task01;

#pragma warning disable CA1859
internal static class Program
{
    /// <summary>Entry point that parses command line options, validates them, and runs the substitution cipher workflow.</summary>
    /// <param name="args">The command line arguments passed to the application.</param>
    /// <returns>An exit code where zero indicates success, one indicates runtime failure, and two indicates validation failure.</returns>
    private static int Main(string[] args)
    {
        IAppOptionsProvider provider = new CommandLineOptionsProvider();
        if (!provider.TryGetOptions(args, out var options, out var parseErrors))
        {
            Printer.Errors(parseErrors);
            Printer.Usage();
            return 2;
        }

        if (options.ShowHelp)
        {
            Printer.Usage();
            return 0;
        }

        var validators = new IOptionsValidator[]
        {
            new AppOptionsValidator(),
            new FileSystemOptionsValidator()
        };

        var allErrors = validators.SelectMany(v => v.Validate(options)).ToList();
        if (allErrors.Count > 0)
        {
            Printer.Errors(allErrors);
            Printer.Usage();
            return 2;
        }

        var reader = new FileReader();
        IRunner runner = new Runner(
            keyLoader: new KeyLoader(reader),
            reader: reader,
            writer: new FileWriter(),
            normalizer: new TextNormalizer(),
            cipher: new SubstitutionCipher());
        return runner.Run(options);
    }
}