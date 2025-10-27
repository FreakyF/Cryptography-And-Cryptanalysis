namespace Task02.Domain.Abstractions;

public sealed class ChiSquareScorer : IChiSquareScorer
{
    static readonly double[] ExpectedFrequencies =
    {
        0.08167, // A
        0.01492, // B
        0.02782, // C
        0.04253, // D
        0.12702, // E
        0.02228, // F
        0.02015, // G
        0.06094, // H
        0.06966, // I
        0.00153, // J
        0.00772, // K
        0.04025, // L
        0.02406, // M
        0.06749, // N
        0.07507, // O
        0.01929, // P
        0.00095, // Q
        0.05987, // R
        0.06327, // S
        0.09056, // T
        0.02758, // U
        0.00978, // V
        0.02360, // W
        0.00150, // X
        0.01974, // Y
        0.00074 // Z
    };

    public double Score(string text)
    {
        if (string.IsNullOrEmpty(text))
            return double.PositiveInfinity;

        var counts = new int[26];
        var span = text.AsSpan();

        foreach (var c in span)
        {
            var idx = c - 'A';
            if ((uint)idx < 26u)
            {
                counts[idx]++;
            }
        }

        var n = span.Length;
        if (n == 0)
        {
            return double.PositiveInfinity;
        }
        var chi2 = 0d;

        for (var i = 0; i < 26; i++)
        {
            var expected = ExpectedFrequencies[i] * n;
            var observed = counts[i];
            var diff = observed - expected;
            chi2 += (diff * diff) / expected;
        }

        return chi2;
    }
}