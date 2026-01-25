namespace Task01.Shared;

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
}