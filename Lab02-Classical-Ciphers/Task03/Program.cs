#pragma warning disable CA1859

using Task03.Application.Abstractions;
using Task03.Application.Models;
using Task03.Application.Services;
using Task03.Domain.Abstractions;
using Task03.Domain.Services;
using Task03.Infrastructure.Services;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

IFileService fileService = new FileService();
IKeyService keyService = new KeyService(fileService);

ITextNormalizer textNormalizer = new TextNormalizer();
IAffineCipher cipher = new AffineCipher();

ICipherOrchestrator orchestrator = new CipherOrchestrator(
    fileService,
    keyService,
    textNormalizer,
    cipher
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

if (!result.IsSuccess && !string.IsNullOrEmpty(result.Message))
{
    await Console.Error.WriteLineAsync(result.Message);
}

Environment.ExitCode = result.ExitCode;