using System.Runtime.CompilerServices;
using System.Text;

namespace Task01.Domain.Utils;

/// <summary>
/// Provides high-performance utility methods for bit manipulation and conversion between different data formats.
/// </summary>
public static class BitConversions
{
    /// <summary>
    /// Converts a UTF-8 string into a sequence of bits.
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <returns>A read-only list of booleans representing the bits of the input string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <remarks>
    /// This method uses <c>stackalloc</c> for small strings to minimize heap allocations.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static IReadOnlyList<bool> StringToBits(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (text.Length == 0)
        {
            return Array.Empty<bool>();
        }

        var maxByteCount = Encoding.UTF8.GetMaxByteCount(text.Length);
        Span<byte> buffer = maxByteCount <= 256
            ? stackalloc byte[maxByteCount]
            : GC.AllocateUninitializedArray<byte>(maxByteCount);

        var bytesWritten = Encoding.UTF8.GetBytes(text.AsSpan(), buffer);

        var bits = GC.AllocateUninitializedArray<bool>(bytesWritten * 8);
        var index = 0;

        for (var i = 0; i < bytesWritten; i++)
        {
            var value = buffer[i];

            for (var bit = 7; bit >= 0; bit--)
            {
                bits[index++] = ((value >> bit) & 1) != 0;
            }
        }

        return bits;
    }

    /// <summary>
    /// Converts a sequence of bits into a UTF-8 string.
    /// </summary>
    /// <param name="bits">The sequence of bits to convert.</param>
    /// <returns>The string representation of the bits.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bits"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static string BitsToString(IEnumerable<bool> bits)
    {
        if (bits == null)
        {
            throw new ArgumentNullException(nameof(bits));
        }

        if (bits is bool[] bitArray)
        {
            return BitsArrayToUtf8String(bitArray);
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

            return BitsArrayToUtf8String(temp);
        }

        var list = bits.ToList();
        if (list.Count == 0)
        {
            return string.Empty;
        }

        var copy = GC.AllocateUninitializedArray<bool>(list.Count);
        for (var i = 0; i < list.Count; i++)
        {
            copy[i] = list[i];
        }

        return BitsArrayToUtf8String(copy);
    }

    /// <summary>
    /// Converts a boolean array of bits to a UTF-8 string.
    /// </summary>
    /// <param name="bits">The boolean array containing bits.</param>
    /// <returns>The decoded UTF-8 string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static string BitsArrayToUtf8String(bool[] bits)
    {
        var bitCount = bits.Length;
        var byteCount = bitCount / 8;

        if (byteCount == 0)
        {
            return string.Empty;
        }

        Span<byte> buffer = byteCount <= 256
            ? stackalloc byte[byteCount]
            : GC.AllocateUninitializedArray<byte>(byteCount);

        var index = 0;

        for (var i = 0; i < byteCount; i++)
        {
            byte value = 0;

            for (var bit = 7; bit >= 0; bit--)
            {
                if (bits[index++])
                {
                    value |= (byte)(1 << bit);
                }
            }

            buffer[i] = value;
        }

        return Encoding.UTF8.GetString(buffer[..byteCount]);
    }

    /// <summary>
    /// Converts a string representation of bits (e.g., "0101") into a sequence of booleans.
    /// </summary>
    /// <param name="bitString">The string containing '0' and '1' characters.</param>
    /// <returns>A read-only list of booleans.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bitString"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the string contains characters other than '0' or '1'.</exception>
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

    /// <summary>
    /// Converts a sequence of booleans into a string of '0's and '1's.
    /// </summary>
    /// <param name="bits">The sequence of bits.</param>
    /// <returns>A string of '0's and '1's.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bits"/> is null.</exception>
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

    /// <summary>
    /// Converts a sequence of integers (0 or 1) into a sequence of booleans.
    /// </summary>
    /// <param name="values">The sequence of integers.</param>
    /// <returns>A read-only list of booleans.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when an integer value is not 0 or 1.</exception>
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

    /// <summary>
    /// Converts a sequence of booleans into a sequence of integers (0 or 1).
    /// </summary>
    /// <param name="bits">The sequence of bits.</param>
    /// <returns>A read-only list of integers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bits"/> is null.</exception>
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
