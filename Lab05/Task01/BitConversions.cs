using System.Text;

namespace Task01;

public static class BitConversions
{
    public static IReadOnlyList<bool> StringToBits(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        var bytes = Encoding.UTF8.GetBytes(text);
        var bits = new List<bool>(bytes.Length * 8);

        foreach (var b in bytes)
        {
            for (var i = 7; i >= 0; i--)
            {
                bits.Add(((b >> i) & 1) == 1);
            }
        }

        return bits;
    }

    public static string BitsToString(IEnumerable<bool> bits)
    {
        if (bits == null)
        {
            throw new ArgumentNullException(nameof(bits));
        }

        var bitList = bits.ToList();
        var byteCount = bitList.Count / 8;
        var bytes = new byte[byteCount];

        for (var i = 0; i < byteCount; i++)
        {
            byte value = 0;
            for (var j = 0; j < 8; j++)
            {
                if (bitList[i * 8 + j])
                {
                    value |= (byte)(1 << (7 - j));
                }
            }

            bytes[i] = value;
        }

        return Encoding.UTF8.GetString(bytes);
    }

    public static IReadOnlyList<bool> BitStringToBits(string bitString)
    {
        if (bitString == null)
        {
            throw new ArgumentNullException(nameof(bitString));
        }

        var bits = new List<bool>(bitString.Length);
        bits.AddRange(bitString.Select(c => c switch
        {
            '0' => false,
            '1' => true,
            _ => throw new ArgumentException("Bit string can contain only '0' or '1'.")
        }));

        return bits;
    }

    public static string BitsToBitString(IEnumerable<bool> bits)
    {
        if (bits == null)
        {
            throw new ArgumentNullException(nameof(bits));
        }

        var builder = new StringBuilder();
        foreach (var bit in bits)
        {
            builder.Append(bit ? '1' : '0');
        }

        return builder.ToString();
    }

    public static IReadOnlyList<bool> IntArrayToBits(IEnumerable<int> values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        return values.Select(v => v switch
        {
            0 => false,
            1 => true,
            _ => throw new ArgumentException("Only 0 and 1 are valid bit values.")
        }).ToArray();
    }

    public static IReadOnlyList<int> BitsToIntArray(IEnumerable<bool> bits)
    {
        return bits == null ? throw new ArgumentNullException(nameof(bits)) : bits.Select(bit => bit ? 1 : 0).ToArray();
    }
}