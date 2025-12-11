namespace Task01;

public sealed class StreamCipher : IStreamCipher
{
    public IReadOnlyList<bool> Encrypt(string plaintext, ILfsr lfsr)
    {
        if (plaintext == null)
        {
            throw new ArgumentNullException(nameof(plaintext));
        }

        if (lfsr == null)
        {
            throw new ArgumentNullException(nameof(lfsr));
        }

        var plainBits = BitConversions.StringToBits(plaintext);
        var cipherBits = new bool[plainBits.Count];

        for (var i = 0; i < plainBits.Count; i++)
        {
            var keyBit = lfsr.NextBit();
            cipherBits[i] = plainBits[i] ^ keyBit;
        }

        return cipherBits;
    }

    public string Decrypt(IReadOnlyList<bool> ciphertextBits, ILfsr lfsr)
    {
        if (ciphertextBits == null)
        {
            throw new ArgumentNullException(nameof(ciphertextBits));
        }

        if (lfsr == null)
        {
            throw new ArgumentNullException(nameof(lfsr));
        }

        var count = ciphertextBits.Count;
        var plainBits = new bool[count];

        for (var i = 0; i < count; i++)
        {
            var keyBit = lfsr.NextBit();
            plainBits[i] = ciphertextBits[i] ^ keyBit;
        }

        return BitConversions.BitsToString(plainBits);
    }
}