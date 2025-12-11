using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace Task01;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && string.Equals(args[0], "benchmark", StringComparison.OrdinalIgnoreCase))
        {
            RunBenchmarks(args.Skip(1).ToArray());
            return;
        }

        IRunner runner = new Runner(false);
        runner.RunAll();
    }

    private static void RunBenchmarks(string[] args)
    {
        var config = ManualConfig
            .CreateEmpty()
            .AddColumnProvider(DefaultColumnProviders.Instance)
            .AddJob(Job.Default);

        var summaries = BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, config);

        var logger = ConsoleLogger.Default;

        foreach (var summary in summaries)
        {
            MarkdownExporter.Console.ExportToLog(summary, logger);
            Console.WriteLine();
        }
    }
}