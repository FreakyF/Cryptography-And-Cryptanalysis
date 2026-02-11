using System.Numerics;

namespace Task01.Domain.Numeric;

public static class ModularArithmetic
{
    public static BigInteger NormalizeMod(BigInteger value, BigInteger modulus)
    {
        var result = value % modulus;

        if (result.Sign < 0)
        {
            result += modulus;
        }

        return result;
    }

    public static (BigInteger Gcd, BigInteger X) ExtendedGcd(BigInteger a, BigInteger b)
    {
        var oldR = a;
        var r = b;
        var oldS = BigInteger.One;
        var s = BigInteger.Zero;

        while (r != BigInteger.Zero)
        {
            var quotient = oldR / r;

            var tempR = oldR - quotient * r;
            oldR = r;
            r = tempR;

            var tempS = oldS - quotient * s;
            oldS = s;
            s = tempS;
        }

        return (BigInteger.Abs(oldR), oldS);
    }

    public static BigInteger? ModInverse(BigInteger value, BigInteger modulus)
    {
        var a = NormalizeMod(value, modulus);

        if (modulus <= BigInteger.One)
        {
            return null;
        }

        var (gcd, x) = ExtendedGcd(a, modulus);

        if (gcd != BigInteger.One)
        {
            return null;
        }

        return NormalizeMod(x, modulus);
    }
}