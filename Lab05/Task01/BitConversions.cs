using System.Runtime.CompilerServices;
using System.Text;

namespace Task01;

[SkipLocalsInit]
public static class BitConversions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static IReadOnlyList<bool> StringToBits(string text)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        if (text.Length == 0)
            return Array.Empty<bool>();

        var bytes = Encoding.UTF8.GetBytes(text);
        var bits = GC.AllocateUninitializedArray<bool>(bytes.Length * 8);
        var index = 0;

        foreach (var b in bytes)
        {
            for (int bit = 7; bit >= 0; bit--)
            {
                bits[index++] = ((b >> bit) & 1) != 0;
            }
        }

        return bits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static string BitsToString(IEnumerable<bool> bits)
    {
        if (bits == null)
            throw new ArgumentNullException(nameof(bits));

        if (bits is bool[] bitArray)
            return BitsArrayToUtf8String(bitArray);

        if (bits is IReadOnlyCollection<bool> collection)
        {
            if (collection.Count == 0)
                return string.Empty;

            var tmp = GC.AllocateUninitializedArray<bool>(collection.Count);
            var idx = 0;
            foreach (var b in collection)
                tmp[idx++] = b;

            return BitsArrayToUtf8String(tmp);
        }

        var list = bits.ToList();
        if (list.Count == 0)
            return string.Empty;

        var copy = GC.AllocateUninitializedArray<bool>(list.Count);
        for (int i = 0; i < list.Count; i++)
            copy[i] = list[i];

        return BitsArrayToUtf8String(copy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static string BitsArrayToUtf8String(bool[] bits)
    {
        int bitCount = bits.Length;
        int byteCount = bitCount / 8;
        if (byteCount == 0)
            return string.Empty;

        var bytes = GC.AllocateUninitializedArray<byte>(byteCount);
        int index = 0;

        for (int i = 0; i < byteCount; i++)
        {
            byte value = 0;

            for (int bit = 7; bit >= 0; bit--)
            {
                if (bits[index++])
                    value |= (byte)(1 << bit);
            }

            bytes[i] = value;
        }

        return Encoding.UTF8.GetString(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static IReadOnlyList<bool> BitStringToBits(string bitString)
    {
        if (bitString == null)
        {
            throw new ArgumentNullException(nameof(bitString));
        }

        var length = bitString.Length;
        if (length == 0)
        {
            return Array.Empty<bool>();
        }

        var bits = GC.AllocateUninitializedArray<bool>(length);

        for (var i = 0; i < length; i++)
        {
            bits[i] = bitString[i] switch
            {
                '0' => false,
                '1' => true,
                _ => throw new ArgumentException("Bit string can contain only '0' or '1'.")
            };
        }

        return bits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static string BitsToBitString(IEnumerable<bool> bits)
    {
        if (bits == null)
        {
            throw new ArgumentNullException(nameof(bits));
        }

        if (bits is bool[] bitArray)
        {
            return string.Create(
                bitArray.Length,
                bitArray,
                static (span, state) =>
                {
                    for (var i = 0; i < span.Length; i++)
                    {
                        span[i] = state[i] ? '1' : '0';
                    }
                });
        }

        if (bits is IReadOnlyCollection<bool> collection)
        {
            var count = collection.Count;
            if (count == 0)
            {
                return string.Empty;
            }

            var temp = GC.AllocateUninitializedArray<bool>(count);
            var index = 0;

            foreach (var bit in collection)
            {
                temp[index++] = bit;
            }

            return string.Create(
                count,
                temp,
                static (span, state) =>
                {
                    for (var i = 0; i < span.Length; i++)
                    {
                        span[i] = state[i] ? '1' : '0';
                    }
                });
        }

        var builder = new StringBuilder();
        foreach (var bit in bits)
        {
            builder.Append(bit ? '1' : '0');
        }

        return builder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static IReadOnlyList<bool> IntArrayToBits(IEnumerable<int> values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (values is int[] intArray)
        {
            var result = GC.AllocateUninitializedArray<bool>(intArray.Length);

            for (var i = 0; i < intArray.Length; i++)
            {
                result[i] = intArray[i] switch
                {
                    0 => false,
                    1 => true,
                    _ => throw new ArgumentException("Only 0 and 1 are valid bit values.")
                };
            }

            return result;
        }

        var list = values as IList<int> ?? values.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<bool>();
        }

        var bits = GC.AllocateUninitializedArray<bool>(list.Count);

        for (var i = 0; i < list.Count; i++)
        {
            bits[i] = list[i] switch
            {
                0 => false,
                1 => true,
                _ => throw new ArgumentException("Only 0 and 1 are valid bit values.")
            };
        }

        return bits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static IReadOnlyList<int> BitsToIntArray(IEnumerable<bool> bits)
    {
        if (bits == null)
        {
            throw new ArgumentNullException(nameof(bits));
        }

        if (bits is bool[] bitArray)
        {
            var result = GC.AllocateUninitializedArray<int>(bitArray.Length);

            for (var i = 0; i < bitArray.Length; i++)
            {
                result[i] = bitArray[i] ? 1 : 0;
            }

            return result;
        }

        if (bits is IReadOnlyCollection<bool> collection)
        {
            var count = collection.Count;
            if (count == 0)
            {
                return Array.Empty<int>();
            }

            var result = GC.AllocateUninitializedArray<int>(count);
            var index = 0;

            foreach (var bit in collection)
            {
                result[index++] = bit ? 1 : 0;
            }

            return result;
        }

        var list = bits.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<int>();
        }

        var array = GC.AllocateUninitializedArray<int>(list.Count);

        for (var i = 0; i < list.Count; i++)
        {
            array[i] = list[i] ? 1 : 0;
        }

        return array;
    }
}