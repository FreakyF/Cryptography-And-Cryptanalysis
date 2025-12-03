using System.Numerics;

namespace Lab01.Domain.Numeric;

public static class PrimalityTest
{
    public static bool IsProbablePrime(BigInteger value, int iterations)
    {
        if (value < 2)
        {
            return false;
        }

        if (value == 2 || value == 3)
        {
            return true;
        }

        if (value.IsEven)
        {
            return false;
        }

        var d = value - 1;
        var r = 0;

        while (d.IsEven)
        {
            d /= 2;
            r++;
        }

        for (var i = 0; i < iterations; i++)
        {
            var a = RandomBigIntegerGenerator.Next(value - 3) + 2;

            if (IsCompositeForBase(value, d, r, a))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsCompositeForBase(BigInteger value, BigInteger d, int r, BigInteger a)
    {
        var x = BigInteger.ModPow(a, d, value);

        if (x == BigInteger.One || x == value - 1)
        {
            return false;
        }

        for (var j = 1; j < r; j++)
        {
            x = BigInteger.ModPow(x, 2, value);

            if (x == value - 1)
            {
                return false;
            }
        }

        return true;
    }
}