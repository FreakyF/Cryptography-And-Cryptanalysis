using Spectre.Console;
using Task04.Domain.Models;

namespace Task04.Application.Services;

public static class ConsoleReport
{
    public static void PrintQualityTable(IReadOnlyList<QualityMetrics> rows)
    {
        PrintSectionHeader("Quality comparison");

        var table = CreateTable();
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

        AnsiConsole.Write(new Align(table, HorizontalAlignment.Left));
    }

    public static void PrintPerformanceTable(IReadOnlyList<PerformanceMetrics> rows)
    {
        PrintSectionHeader("Performance comparison");

        var table = CreateTable();
        table.AddColumn("Algorithm");
        table.AddColumn("Min iters for 85%");
        table.AddColumn("Mean time (ms) - 10 runs");
        table.AddColumn("Mean text acc (%)");

        foreach (var r in rows)
        {
            var alg = Markup.Escape(r.Algorithm);
            var iters = r.MinIterations.HasValue ? r.MinIterations.Value.ToString() : "[grey]not reached[/]";
            var mean = r.MeanTimeMs.HasValue ? $"{r.MeanTimeMs.Value:0.000}" : "[grey]N/A[/]";
            var acc = r.MeanTextAccuracy.HasValue ? $"{r.MeanTextAccuracy.Value:0.00}" : "[grey]N/A[/]";

            table.AddRow(alg, iters, mean, acc);
        }

        AnsiConsole.Write(new Align(table, HorizontalAlignment.Left));
    }

    public static void PrintConvergenceHeader()
    {
        PrintSectionHeader("Convergence analysis");
    }

    private static void PrintSectionHeader(string title)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(title)}[/]");
    }

    private static Table CreateTable()
    {
        return new Table
        {
            Border = TableBorder.Rounded,
            Expand = false
        };
    }
}