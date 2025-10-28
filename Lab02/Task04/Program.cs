#pragma warning disable CA1859

using Task04.Application.Abstractions;
using Task04.Application.Models;
using Task04.Application.Services;
using Task04.Domain.Abstractions;
using Task04.Domain.Services;
using Task04.Infrastructure.Services;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

IFileService fileService = new FileService();
IKeyService keyProvider = new KeyService(fileService);

ITextNormalizer textNormalizer = new TextNormalizer();
IAffineCipher cipher = new AffineCipher();
IChiSquareScorer scorer = new ChiSquareScorer();
IBruteForceAttack brute = new BruteForceAttack(cipher, scorer);

ICipherOrchestrator orchestrator = new CipherOrchestrator(
    fileService,
    keyProvider,
    textNormalizer,
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