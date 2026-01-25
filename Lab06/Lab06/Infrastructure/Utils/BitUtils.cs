namespace Lab06.Infrastructure.Utils;

public static class BitUtils
{
    public static int[] StringToBits(string text)
    {
        return BytesToBits(Encoding.UTF8.GetBytes(text));
    }

    public static string BitsToString(int[] bits)
    {
        return Encoding.UTF8.GetString(BitsToBytes(bits));
    }

    public static int[] BytesToBits(byte[] bytes)
    {
        var bits = new List<int>(bytes.Length * 8);
        foreach (var b in bytes)
        {
            for (var i = 7; i >= 0; i--)
            {
                bits.Add((b >> i) & 1);
            }
        }

        return bits.ToArray();
    }

    public static byte[] BitsToBytes(int[] bits)
    {
        if (bits.Length % 8 != 0)
        {
            throw new ArgumentException("Bit length must be multiple of 8");
        }

        var byteCount = bits.Length / 8;
        var bytes = new byte[byteCount];

        for (var i = 0; i < byteCount; i++)
        {
            byte b = 0;
            for (var j = 0; j < 8; j++)
            {
                if (bits[i * 8 + j] == 1)
                {
                    b |= (byte)(1 << (7 - j));
                }
            }

            bytes[i] = b;
        }

        return bytes;
    }

    public static int[] IntToBinaryArray(int value, int length)
    {
        var result = new int[length];
        for (var i = 0; i < length; i++)
        {
            result[length - 1 - i] = (value >> i) & 1;
        }

        return result;
    }

    public static int[] GenerateLfsrSequence(int length, int[] startState, int[] taps)
    {
        var seq = new int[length];
        var state = startState.ToArray();
        var degree = state.Length;

        for (var i = 0; i < length; i++)
        {
            seq[i] = state[0];
            var feedback = taps.Aggregate(0, (current, t) => current ^ state[t]);

            Array.Copy(state, 1, state, 0, degree - 1);
            state[degree - 1] = feedback;
        }

        return seq;
    }
}