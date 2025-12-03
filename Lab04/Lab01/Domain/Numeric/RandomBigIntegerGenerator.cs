using System.Numerics;
using System.Security.Cryptography;

namespace Lab01.Domain.Numeric;

public static class RandomBigIntegerGenerator
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    public static BigInteger Next(BigInteger upperExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(upperExclusive, BigInteger.Zero);

        var bytes = upperExclusive.ToByteArray(true, true);

        if (bytes.Length == 0)
        {
            bytes = new byte[1];
        }

        while (true)
        {
            var buffer = new byte[bytes.Length];
            Rng.GetBytes(buffer);
            var candidate = new BigInteger(buffer, true, true);

            if (candidate < upperExclusive)
            {
                return candidate;
            }
        }
    }

    public static BigInteger Next(BigInteger lowerInclusive, BigInteger upperExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lowerInclusive, upperExclusive);

        var range = upperExclusive - lowerInclusive;
        var offset = Next(range);
        return lowerInclusive + offset;
    }
}