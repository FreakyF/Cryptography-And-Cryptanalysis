using AsciiChart.Sharp;
using Spectre.Console;
using Task04.Domain.Models;

namespace Task04.Application.Services;

public static class AsciiLineChartsRenderer
{
    public static void RenderMetric(
        string title,
        ConvergenceSeries mh,
        ConvergenceSeries sa,
        Func<ConvergencePoint, double> selector,
        int height = 12,
        int width = 100)
    {
        var mhValues = mh.Points.Select(selector).ToArray();
        var saValues = sa.Points.Select(selector).ToArray();
        var iters = mh.Points.Select(p => p.Iterations).ToArray();

        mhValues = Downsample(mhValues, width);
        saValues = Downsample(saValues, width);
        var itersLegend = Downsample(iters.Select(i => (double)i).ToArray(), width)
            .Select(d => (int)Math.Round(d))
            .ToArray();

        var opts = new Options { Height = height };

        var mhChart = AsciiChart.Sharp.AsciiChart.Plot(mhValues, opts);
        var saChart = AsciiChart.Sharp.AsciiChart.Plot(saValues, opts);

        var mhPlotWidth = MaxLineLen(mhChart);
        var saPlotWidth = MaxLineLen(saChart);
        var targetPlotWidth = Math.Max(mhPlotWidth, saPlotWidth);

        var mhLegend = BuildXLegend(itersLegend, targetPlotWidth);
        var saLegend = BuildXLegend(itersLegend, targetPlotWidth);

        var leftBlock =
            $"{title} — MH (mean)\n" +
            mhChart + "\n" +
            mhLegend;

        var rightBlock =
            $"{title} — SA (mean)\n" +
            saChart + "\n" +
            saLegend;

        var targetBlockWidth = Math.Max(MaxLineLen(leftBlock), MaxLineLen(rightBlock));
        leftBlock = PadBlockToWidth(leftBlock, targetBlockWidth);
        rightBlock = PadBlockToWidth(rightBlock, targetBlockWidth);

        var leftPanel = new Panel(leftBlock).Border(BoxBorder.Rounded);
        var rightPanel = new Panel(rightBlock).Border(BoxBorder.Rounded);

        AnsiConsole.Write(new Columns(leftPanel, rightPanel));
    }

    private static T[] Downsample<T>(T[] data, int maxLen)
    {
        if (data.Length <= maxLen)
        {
            return data;
        }

        var res = new T[maxLen];
        var s = (double)data.Length / maxLen;
        for (var i = 0; i < maxLen; i++)
        {
            var idx = (int)Math.Min(data.Length - 1, Math.Round(i * s));
            res[i] = data[idx];
        }

        return res;
    }

    private static string BuildXLegend(int[] iters, int plotWidth)
    {
        if (iters.Length == 0)
        {
            return string.Empty;
        }

        const string prefix = "iters: ";
        var padWidth = Math.Max(0, plotWidth - prefix.Length);
        var line = new char[padWidth];
        Array.Fill(line, ' ');

        var n = iters.Length;
        int[] anchors = [0, n / 2, n - 1];
        var labels = anchors.Select(i => iters[i].ToString()).ToArray();

        const int p0 = 0;
        var p1 = Math.Max(labels[0].Length + 2, padWidth / 2 - labels[1].Length / 2);
        var p2 = Math.Max(p1 + labels[1].Length + 2, padWidth - labels[2].Length);

        Insert(line, labels[0], p0);
        Insert(line, labels[1], p1);
        Insert(line, labels[2], p2);

        return prefix + new string(line);
    }

    private static void Insert(char[] dst, string text, int start)
    {
        for (var i = 0; i < text.Length && start + i < dst.Length; i++)
        {
            dst[start + i] = text[i];
        }
    }

    private static int MaxLineLen(string block)
    {
        return block.Replace("\r", "").Split('\n').Max(l => l.Length);
    }

    private static string PadBlockToWidth(string block, int width)
    {
        var lines = block.Replace("\r", "").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length < width)
            {
                lines[i] = lines[i].PadRight(width);
            }
        }

        return string.Join('\n', lines);
    }
}