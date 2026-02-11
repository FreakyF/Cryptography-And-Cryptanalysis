namespace Lab06.Infrastructure.Utils;

/// <summary>
/// Provides utility methods for bit manipulation and conversion.
/// </summary>
public static class BitUtils
{
    /// <summary>
    /// Converts a string to an array of bits using UTF-8 encoding.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <returns>An array of integers representing the bits.</returns>
    public static int[] StringToBits(string text)
    {
        return BytesToBits(Encoding.UTF8.GetBytes(text));
    }

    /// <summary>
    /// Converts an array of bits back to a string using UTF-8 encoding.
    /// </summary>
    /// <param name="bits">The input array of bits.</param>
    /// <returns>The resulting string.</returns>
    public static string BitsToString(int[] bits)
    {
        return Encoding.UTF8.GetString(BitsToBytes(bits));
    }

    /// <summary>
    /// Converts a byte array to an array of bits.
    /// </summary>
    /// <param name="bytes">The input byte array.</param>
    /// <returns>An array of integers (0 or 1) representing the bits, with MSB first.</returns>
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

    /// <summary>
    /// Converts an array of bits to a byte array.
    /// </summary>
    /// <param name="bits">The input array of bits. Length must be a multiple of 8.</param>
    /// <returns>The resulting byte array.</returns>
    /// <exception cref="ArgumentException">Thrown when the bit array length is not a multiple of 8.</exception>
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

    /// <summary>
    /// Converts an integer to a binary array of a specified length.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <param name="length">The length of the resulting binary array.</param>
    /// <returns>An array of integers representing the binary value.</returns>
    public static int[] IntToBinaryArray(int value, int length)
    {
        var result = new int[length];
        for (var i = 0; i < length; i++)
        {
            result[length - 1 - i] = (value >> i) & 1;
        }

        return result;
    }

    /// <summary>
    /// Generates a sequence from an LFSR given an initial state and taps without modifying the state object.
    /// </summary>
    /// <param name="length">The number of bits to generate.</param>
    /// <param name="startState">The initial state of the LFSR.</param>
    /// <param name="taps">The feedback taps.</param>
    /// <returns>The generated sequence of bits.</returns>
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
