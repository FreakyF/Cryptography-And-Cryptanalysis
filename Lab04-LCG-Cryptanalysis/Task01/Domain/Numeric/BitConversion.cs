using System.Numerics;
using System.Text;

namespace Task01.Domain.Numeric;

public static class BitConversion
{
    public static bool[] StringToBits(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var bits = new bool[bytes.Length * 8];

        for (var i = 0; i < bytes.Length; i++)
        {
            var b = bytes[i];

            for (var j = 0; j < 8; j++)
            {
                var bitIndex = i * 8 + j;
                var mask = 1 << (7 - j);
                bits[bitIndex] = (b & mask) != 0;
            }
        }

        return bits;
    }

    public static string BitsToString(IReadOnlyList<bool> bits)
    {
        var bitCount = bits.Count;

        if (bitCount % 8 != 0)
        {
            throw new ArgumentException("Bit count must be divisible by 8.");
        }

        var bytes = new byte[bitCount / 8];

        for (var i = 0; i < bytes.Length; i++)
        {
            byte value = 0;

            for (var j = 0; j < 8; j++)
            {
                value <<= 1;

                if (bits[i * 8 + j])
                {
                    value |= 1;
                }
            }

            bytes[i] = value;
        }

        return Encoding.UTF8.GetString(bytes);
    }

    public static bool[] ToFixedSizeBits(BigInteger value, int bitLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bitLength);

        if (value.Sign < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        var bits = new bool[bitLength];

        for (var i = 0; i < bitLength; i++)
        {
            var mask = BigInteger.One << i;
            bits[bitLength - 1 - i] = (value & mask) != BigInteger.Zero;
        }

        return bits;
    }

    public static BigInteger BitsToBigInteger(IReadOnlyList<bool> bits)
    {
        var result = BigInteger.Zero;

        foreach (var t in bits)
        {
            result <<= 1;

            if (t)
            {
                result += BigInteger.One;
            }
        }

        return result;
    }

    public static bool[] Slice(bool[] source, int offset, int length)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (offset < 0 || length < 0 || offset + length > source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        var result = new bool[length];
        Array.Copy(source, offset, result, 0, length);
        return result;
    }
}