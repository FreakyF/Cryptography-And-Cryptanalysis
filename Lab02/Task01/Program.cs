#pragma warning disable CA1859

using Task01.Application.Abstractions;
using Task01.Application.Models;
using Task01.Application.Services;
using Task01.Domain.Abstractions;
using Task01.Domain.Services;
using Task01.Infrastructure.Services;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

IFileService fileService = new FileService();
IKeyProvider keyProvider = new KeyService(fileService);

ITextNormalizer textNormalizer = new TextNormalizer();
ICaesarCipher cipher = new CaesarCipher();

ICipherOrchestrator orchestrator = new CipherOrchestrator(
    fileService,
    keyProvider,
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