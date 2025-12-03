using System.Numerics;
using Lab01.Domain.Numeric;

namespace Lab01.Domain.Cryptography;

public sealed class StreamCipher : IStreamCipher
{
    public bool[] Encrypt(string plaintext, IKeyStreamGenerator generator)
    {
        var bits = BitConversion.StringToBits(plaintext);
        var result = new bool[bits.Length];

        for (var i = 0; i < bits.Length; i++)
        {
            var keyBit = generator.NextBit();
            result[i] = bits[i] ^ keyBit;
        }

        return result;
    }

    public string Decrypt(IReadOnlyList<bool> ciphertext, BigInteger seed, IKeyStreamGenerator generator)
    {
        generator.Reset(seed);
        var bits = new bool[ciphertext.Count];

        for (var i = 0; i < ciphertext.Count; i++)
        {
            var keyBit = generator.NextBit();
            bits[i] = ciphertext[i] ^ keyBit;
        }

        return BitConversion.BitsToString(bits);
    }
}