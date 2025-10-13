using Task02.Application;
using Task02.Application.Abstractions;
using Task02.Application.Analysis;
using Task02.Application.Cipher;
using Task02.Application.Reference;
using Task02.Application.Text;
using Task02.Application.Validation;
using Task02.Infrastructure.CLI;
using Task02.Infrastructure.Common;
using Task02.Infrastructure.IO;
using Task02.Infrastructure.Validation;

namespace Task02;

#pragma warning disable CA1859
internal static class Program
{
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
            cipher: new SubstitutionCipher(),
            ngramCounter: new NGramCounter(),
            referenceLoader: new ReferenceLoader(reader),
            chiSquare: new ChiSquareCalculator(new NGramCounter()));
        return runner.Run(options);
    }
}