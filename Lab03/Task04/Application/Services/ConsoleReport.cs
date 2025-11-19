using Spectre.Console;
using Task04.Domain.Models;

namespace Task04.Application.Services;

public static class ConsoleReport
{
    public static void PrintQualityTable(IReadOnlyList<QualityMetrics> rows)
    {
        AnsiConsole.WriteLine();
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.Title = new TableTitle("[bold]Quality comparison[/]");
        table.AddColumn("Algorithm");
        table.AddColumn("Text accuracy");
        table.AddColumn("Key accuracy");

        foreach (var r in rows)
        {
            table.AddRow(
                r.Algorithm,
                $"{r.TextAccuracyPercent:0.00} %",
                r.KeyAccuracyPercent.HasValue ? $"{r.KeyAccuracyPercent.Value:0.00} %" : "N/A"
            );
        }

        AnsiConsole.Write(table);
    }
}