namespace Task01.Shared;

/// <summary>
///     Provides extension methods for bitwise manipulations and format conversions.
/// </summary>
public static class BitExtensions
{
    /// <summary>
    ///     Converts a hexadecimal string representation into an array of boolean values representing individual bits.
    /// </summary>
    /// <param name="hex">
    ///     The input string containing hexadecimal characters. The string length must be even.
    /// </param>
    /// <returns>
    ///     A boolean array where each element represents a bit from the parsed byte array.
    ///     The bits are extracted in little-endian order within each byte (LSB at index 0 relative to the byte).
    /// </returns>
    /// <exception cref="FormatException">
    ///     Thrown if the <paramref name="hex"/> string length is not a multiple of 2 or contains invalid hexadecimal characters.
    /// </exception>
    public static bool[] HexToBits(this string hex)
    {
        var bytes = Convert.FromHexString(hex);
        var bits = new bool[bytes.Length * 8];
        for (var i = 0; i < bytes.Length; i++)
        {
            var b = bytes[i];
            for (var j = 0; j < 8; j++)
            {
                bits[i * 8 + j] = ((b >> j) & 1) == 1;
            }
        }

        return bits;
    }
}
