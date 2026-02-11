using System.Numerics;

namespace Task01.Domain.Numeric;

public static class PrimeGenerator
{
    private static BigInteger FindPrimeAtOrAbove(BigInteger start, int iterations)
    {
        var candidate = start;

        if (candidate.IsEven)
        {
            candidate += BigInteger.One;
        }

        while (true)
        {
            if (PrimalityTest.IsProbablePrime(candidate, iterations))
            {
                return candidate;
            }

            candidate += 2;
        }
    }

    public static BigInteger FindPrimeNearPowerOfTwo(int bitLength, int iterations)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(bitLength, 1);

        var upperExclusive = BigInteger.One << bitLength;
        var candidate = upperExclusive - BigInteger.One;

        if (candidate.IsEven)
        {
            candidate -= BigInteger.One;
        }

        var lowerBound = BigInteger.One << (bitLength - 1);

        while (candidate > lowerBound)
        {
            if (PrimalityTest.IsProbablePrime(candidate, iterations))
            {
                return candidate;
            }

            candidate -= 2;
        }

        return FindPrimeAtOrAbove(lowerBound + 1, iterations);
    }
}