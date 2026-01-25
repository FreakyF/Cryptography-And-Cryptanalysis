namespace Lab06;

public class CryptoSystem(IStreamGenerator generator)
{
    public int[] Encrypt(string plainText)
    {
        var messageBits = BitUtils.StringToBits(plainText);
        return ProcessBits(messageBits);
    }

    public string Decrypt(int[] cipherBits)
    {
        var plainBits = ProcessBits(cipherBits);
        return BitUtils.BitsToString(plainBits);
    }

    public void EncryptFile(string inputPath, string outputPath)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException($"Input file not found: {inputPath}");

        var inputBytes = File.ReadAllBytes(inputPath);
        var inputBits = BitUtils.BytesToBits(inputBytes);
        
        var cipherBits = ProcessBits(inputBits);
        
        var cipherBytes = BitUtils.BitsToBytes(cipherBits);
        File.WriteAllBytes(outputPath, cipherBytes);
    }

    public void DecryptFile(string inputPath, string outputPath)
    {
        EncryptFile(inputPath, outputPath);
    }

    private int[] ProcessBits(int[] inputBits)
    {
        var outputBits = new int[inputBits.Length];
        for (var i = 0; i < inputBits.Length; i++)
        {
            var keyBit = generator.NextBit();
            outputBits[i] = inputBits[i] ^ keyBit;
        }
        return outputBits;
    }

    public static int[] RecoverKeystream(string knownPlaintext, int[] cipherBits)
    {
        var plainBits = BitUtils.StringToBits(knownPlaintext);
        if (plainBits.Length != cipherBits.Length)
        {
            throw new ArgumentException("Length mismatch");
        }

        var keystream = new int[plainBits.Length];
        for (var i = 0; i < plainBits.Length; i++)
        {
            keystream[i] = plainBits[i] ^ cipherBits[i];
        }
        return keystream;
    }
}