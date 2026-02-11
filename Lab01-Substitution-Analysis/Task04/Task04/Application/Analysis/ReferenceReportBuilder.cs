using System.Globalization;

namespace Task04.Application.Analysis;

public static class ReferenceReportBuilder
{
    /// <summary>Formats n-gram counts into a probability table sorted by frequency then gram name.</summary>
    /// <param name="counts">The dictionary of n-gram counts used to compute probabilities.</param>
    /// <returns>A newline-delimited string with each n-gram and its probability.</returns>
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