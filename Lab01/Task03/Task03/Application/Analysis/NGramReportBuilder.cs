using System.Globalization;

namespace Task03.Application.Analysis;

public static class NGramReportBuilder
{
    public static string Build(IReadOnlyDictionary<string, int> counts)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var kv in counts
                     .OrderByDescending(kv => kv.Value)
                     .ThenBy(kv => kv.Key, StringComparer.Ordinal))
        {
            sb.Append(CultureInfo.InvariantCulture, $"{kv.Key} {kv.Value}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}