namespace Task01.Shared;

using System.Text;

public static class BitExtensions
{
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

    // ReSharper disable once ConvertToExtensionBlock
    public static string BitsToHex(this bool[] bits)
    {
        var bytes = bits.ToByteArray();
        return Convert.ToHexString(bytes);
    }

    public static byte[] ToByteArray(this bool[] bits)
    {
        var byteCount = (bits.Length + 7) / 8;
        var bytes = new byte[byteCount];
        for (var i = 0; i < bits.Length; i++)
        {
            if (bits[i])
            {
                bytes[i / 8] |= (byte)(1 << (i % 8));
            }
        }
        return bytes;
    }

    public static string ToAsciiString(this byte[] bytes) => Encoding.ASCII.GetString(bytes);
}