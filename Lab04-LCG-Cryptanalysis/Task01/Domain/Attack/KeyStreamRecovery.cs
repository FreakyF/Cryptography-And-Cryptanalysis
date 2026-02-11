using Task01.Domain.Numeric;

namespace Task01.Domain.Attack;

public static class KeyStreamRecovery
{
    public static bool[] RecoverFromKnownPlaintext(string knownPlaintext, IReadOnlyList<bool> ciphertext)
    {
        var knownBits = BitConversion.StringToBits(knownPlaintext);

        if (ciphertext.Count < knownBits.Length)
        {
            throw new ArgumentException("Ciphertext shorter than known plaintext bits.");
        }

        var result = new bool[knownBits.Length];

        for (var i = 0; i < knownBits.Length; i++)
        {
            result[i] = knownBits[i] ^ ciphertext[i];
        }

        return result;
    }
}