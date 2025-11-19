using Task04.Application.Models;
using Task04.Application.Services;
using Task04.Domain.Services;
using Task04.Infrastructure.Services;

var argsModel = new QualityComparisonArgs(
    NextOf("--task02", args) ?? throw new ArgumentException("--task02 is required"),
    NextOf("--task03", args) ?? throw new ArgumentException("--task03 is required"),
    NextOf("-i", args) ?? throw new ArgumentException("-i is required"),
    NextOf("-r", args) ?? throw new ArgumentException("-r is required"),
    NextOf("-p", args) ?? throw new ArgumentException("-p is required"),
    NextOf("-k", args),
    NextOf("-o", args) ?? "."
);

Directory.CreateDirectory(argsModel.WorkDir);

var process = new ProcessRunner();
var normalizer = new TextNormalizer();
var evaluator = new QualityEvaluator(normalizer);
var algoMh = new ExternalRunner(process, argsModel.Task02ExePath, "MH");
var algoSa = new ExternalRunner(process, argsModel.Task03ExePath, "SA");

var orchestrator = new QualityComparisonOrchestrator(
    algoMh, algoSa, evaluator
);

await orchestrator.RunAsync(argsModel);
return;

static string? NextOf(string key, string[] args)
{
    var i = Array.IndexOf(args, key);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}