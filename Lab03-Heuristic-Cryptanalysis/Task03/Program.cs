#pragma warning disable CA1859

using System.Runtime;
using Task03.Application.Abstractions;
using Task03.Application.Models;
using Task03.Application.Services;
using Task03.Domain.Abstractions;
using Task03.Domain.Services;
using Task03.Infrastructure.Services;

GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

IFileService fileService = new FileService();
IKeyService keyService = new KeyService();

ITextNormalizer textNormalizer = new TextNormalizer();
ISubstitutionCipher cipher = new SubstitutionCipher();
IHeuristicAnalyzer heuristicAnalyzer = new SimulatedAnnealingAnalyzer(textNormalizer, cipher);

ICipherOrchestrator orchestrator = new CipherOrchestrator(
    fileService, keyService, textNormalizer, cipher, heuristicAnalyzer);

IArgumentParser parser = new ArgumentParser();

ProcessingResult result;
try
{
    var parsed = parser.Parse(args);
    result = orchestrator.Run(parsed);
}
catch (ArgumentException ex)
{
    result = new ProcessingResult(1, ex.Message);
}
catch
{
    result = new ProcessingResult(99, "Unexpected error");
}

if (!result.IsSuccess && !string.IsNullOrEmpty(result.Message))
{
    Console.Error.WriteLine(result.Message);
}

Environment.ExitCode = result.ExitCode;