namespace Lab06.Infrastructure.Utils;

/// <summary>
/// Provides statistical utility methods.
/// </summary>
public static class Statistics
{
    /// <summary>
    /// Calculates the Pearson correlation coefficient between two integer arrays.
    /// </summary>
    /// <param name="x">The first array of values.</param>
    /// <param name="y">The second array of values.</param>
    /// <returns>The Pearson correlation coefficient (between -1 and 1).</returns>
    /// <exception cref="ArgumentException">Thrown when the input arrays do not have the same length.</exception>
    public static double PearsonCorrelation(int[] x, int[] y)
    {
        if (x.Length != y.Length)
        {
            throw new ArgumentException("Arrays must have same length");
        }

        var n = x.Length;
        double sumX = x.Sum();
        double sumY = y.Sum();

        var meanX = sumX / n;
        var meanY = sumY / n;

        double numerator = 0;
        double sumSqDiffX = 0;
        double sumSqDiffY = 0;

        for (var i = 0; i < n; i++)
        {
            var diffX = x[i] - meanX;
            var diffY = y[i] - meanY;
            numerator += diffX * diffY;
            sumSqDiffX += diffX * diffX;
            sumSqDiffY += diffY * diffY;
        }

        var denominator = Math.Sqrt(sumSqDiffX) * Math.Sqrt(sumSqDiffY);

        if (Math.Abs(denominator) < 1e-9)
        {
            return 0;
        }

        return numerator / denominator;
    }
}
