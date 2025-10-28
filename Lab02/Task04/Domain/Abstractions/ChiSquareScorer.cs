namespace Task04.Domain.Abstractions;

public sealed class ChiSquareScorer : IChiSquareScorer
{
    private static readonly double[] Expected =
    {
        0.08167, 0.01492, 0.02782, 0.04253, 0.12702, 0.02228, 0.02015,
        0.06094, 0.06966, 0.00153, 0.00772, 0.04025, 0.02406, 0.06749,
        0.07507, 0.01929, 0.00095, 0.05987, 0.06327, 0.09056, 0.02758,
        0.00978, 0.02360, 0.00150, 0.01974, 0.00074
    };

    public double Score(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return double.PositiveInfinity;
        }

        var counts = new int[26];
        var s = text.AsSpan();
        foreach (var t in s)
        {
            var idx = t - 'A';
            if ((uint)idx < 26u)
            {
                counts[idx]++;
            }
        }

        var n = s.Length;
        if (n == 0)
        {
            return double.PositiveInfinity;
        }

        double chi2 = 0;
        for (var i = 0; i < 26; i++)
        {
            var exp = Expected[i] * n;
            var diff = counts[i] - exp;
            chi2 += diff * diff / exp;
        }

        return chi2;
    }
}