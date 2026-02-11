using System.Globalization;

namespace Task02.Application.Analysis;

public static class NGramReportBuilder
{
    /// <summary>Formats n-gram counts into a descending frequency report with lexicographic tie-breaking.</summary>
    /// <param name="counts">The n-gram frequency dictionary to format.</param>
    /// <returns>A newline-delimited string where each line contains an n-gram and its count.</returns>
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