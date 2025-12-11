namespace Task01;

public sealed class BerlekampMasseySolver : IBerlekampMasseySolver
{
    public BerlekampMasseyResult Solve(IReadOnlyList<bool> sequence)
    {
        if (sequence == null)
        {
            throw new ArgumentNullException(nameof(sequence));
        }

        var length = sequence.Count;

        var c = new List<bool> { true };
        var b = new List<bool> { true };
        var l = 0;
        var m = -1;

        for (var index = 0; index < length; index++)
        {
            var discrepancy = sequence[index];

            for (var i = 1; i <= l; i++)
            {
                if (i < c.Count && c[i] && sequence[index - i])
                {
                    discrepancy ^= true;
                }
            }

            if (!discrepancy)
            {
                continue;
            }

            var previousConnectionPolynomial = new List<bool>(c);
            var delta = index - m;

            EnsureLength(c, b.Count + delta);

            for (var i = 0; i < b.Count; i++)
            {
                c[delta + i] ^= b[i];
            }

            if (2 * l > index)
            {
                continue;
            }

            l = index + 1 - l;
            b = previousConnectionPolynomial;
            m = index;
        }

        var trimmed = c.Take(l + 1).ToArray();
        return new BerlekampMasseyResult(trimmed, l);
    }

    private static void EnsureLength(List<bool> list, int length)
    {
        while (list.Count < length)
        {
            list.Add(false);
        }
    }
}