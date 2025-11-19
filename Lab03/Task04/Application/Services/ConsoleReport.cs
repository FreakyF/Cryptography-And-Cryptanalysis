using Spectre.Console;
using Task04.Domain.Models;

namespace Task04.Application.Services;

public static class ConsoleReport
{
    public static void PrintQualityTable(IReadOnlyList<QualityMetrics> rows)
    {
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.Rounded).Title("[bold]Quality comparison[/]");
        table.AddColumn("Algorithm");
        table.AddColumn("Text accuracy");
        table.AddColumn("Key accuracy");

        foreach (var r in rows)
        {
            table.AddRow(
                Markup.Escape(r.Algorithm),
                $"{r.TextAccuracyPercent:0.00} %",
                r.KeyAccuracyPercent.HasValue ? $"{r.KeyAccuracyPercent.Value:0.00} %" : "N/A"
            );
        }

        AnsiConsole.Write(table);
    }

    public static void PrintPerformanceTable(IReadOnlyList<PerformanceMetrics> rows)
    {
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.Rounded).Title("[bold]Performance comparison[/]");
        table.AddColumn("Algorithm");
        table.AddColumn("Min iters for 85%");
        table.AddColumn("Mean time (ms) - 10 runs");
        table.AddColumn("Mean text acc (%)");

        foreach (var r in rows)
        {
            var alg = Markup.Escape(r.Algorithm);

            var iters = r.MinIterations.HasValue
                ? r.MinIterations.Value.ToString()
                : "[grey]not reached[/]";

            var meanTime = r.MeanTimeMs.HasValue
                ? $"{r.MeanTimeMs.Value:0.000}"
                : "[grey]N/A[/]";

            var meanAcc = r.MeanTextAccuracy.HasValue
                ? $"{r.MeanTextAccuracy.Value:0.00}"
                : "[grey]N/A[/]";

            table.AddRow(alg, iters, meanTime, meanAcc);
        }

        AnsiConsole.Write(table);
    }
}