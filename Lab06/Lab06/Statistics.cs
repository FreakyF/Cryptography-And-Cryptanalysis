namespace Lab06;

public static class Statistics
{
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