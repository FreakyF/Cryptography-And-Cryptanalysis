#pragma warning disable CA1859

using Task02.Application.Abstractions;
using Task02.Application.Models;
using Task02.Application.Services;
using Task02.Domain.Abstractions;
using Task02.Domain.Services;
using Task02.Infrastructure.Services;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

IFileService fileService = new FileService();
IKeyService keyService = new KeyService(fileService);

ITextNormalizer normalizer = new TextNormalizer();
ICaesarCipher cipher = new CaesarCipher();
IChiSquareScorer scorer = new ChiSquareScorer();
IBruteForceAttack brute = new BruteForceAttack(cipher, scorer);

ICipherOrchestrator orchestrator = new CipherOrchestrator(
    fileService,
    keyService,
    normalizer,
    cipher,
    brute
);

IArgumentParser parser = new ArgumentParser();

ProcessingResult result;

try
{
    var parsed = parser.Parse(args);

    result = await orchestrator.RunAsync(parsed);
}
catch (ArgumentException ex)
{
    result = new ProcessingResult(1, ex.Message);
}
catch (Exception)
{
    result = new ProcessingResult(99, "Unexpected error");
}

if (!string.IsNullOrEmpty(result.Message))
{
    await Console.Error.WriteLineAsync(result.Message);
}

Environment.ExitCode = result.ExitCode;