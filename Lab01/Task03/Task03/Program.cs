using Task03.Application;
using Task03.Application.Abstractions;
using Task03.Application.Analysis;
using Task03.Application.Cipher;
using Task03.Application.Reference;
using Task03.Application.Text;
using Task03.Application.Validation;
using Task03.Infrastructure.CLI;
using Task03.Infrastructure.Common;
using Task03.Infrastructure.IO;
using Task03.Infrastructure.Validation;

namespace Task03;

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
        var ngrams = new NGramCounter();

        var analysis = new AnalysisServices(
            nGramCounter: ngrams,
            referenceLoader: new ReferenceLoader(reader),
            chiSquare: new ChiSquareCalculator(ngrams));

        IRunner runner = new Runner(
            keyLoader: new KeyLoader(reader),
            reader: reader,
            writer: new FileWriter(),
            normalizer: new TextNormalizer(),
            cipher: new SubstitutionCipher(),
            analysis: analysis);

        return runner.Run(options);
    }
}