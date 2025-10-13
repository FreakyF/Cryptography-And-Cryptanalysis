using System.Globalization;

namespace Task03.Application.Analysis;

public static class ReferenceReportBuilder
{
    public static string BuildProbabilities(IReadOnlyDictionary<string, int> counts)
    {
        var total = counts.Values.Sum();
        if (total == 0) return string.Empty;

        var sb = new System.Text.StringBuilder();
        foreach (var kv in counts
                     .OrderByDescending(k => k.Value)
                     .ThenBy(k => k.Key, StringComparer.Ordinal))
        {
            var p = (double)kv.Value / total;
            sb.Append(CultureInfo.InvariantCulture, $"{kv.Key} {p}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}