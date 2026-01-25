using System.Diagnostics.CodeAnalysis;

namespace Lab06;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
// ReSharper disable once ClassNeverInstantiated.Global
internal class Program
{
    private static void Main()
    {
        Console.WriteLine("=== PHASE I: Key Generation ===");
        var rnd = new Random();

        var keyX = GenerateNonZeroState(3, rnd);
        var keyY = GenerateNonZeroState(4, rnd);
        var keyZ = GenerateNonZeroState(5, rnd);

        Console.WriteLine($"Original X: {string.Join("", keyX)}");
        Console.WriteLine($"Original Y: {string.Join("", keyY)}");
        Console.WriteLine($"Original Z: {string.Join("", keyZ)}");

        var lfsrX = new Lfsr(3, [0, 1], keyX);
        var lfsrY = new Lfsr(4, [0, 3], keyY);
        var lfsrZ = new Lfsr(5, [0, 2], keyZ);

        IStreamGenerator generator = new CombinationGenerator(lfsrX, lfsrY, lfsrZ);
        var cryptoSystem = new CryptoSystem(generator);

        Console.WriteLine("\n=== PHASE II: Encryption ===");
        const string plainText = "Kryptografia jest fascynująca";

        Console.WriteLine($"Plaintext: {plainText}");
        var cipherTextBits = cryptoSystem.Encrypt(plainText);
        Console.WriteLine($"Ciphertext (bits): {cipherTextBits.Length} bits generated.");

        Console.WriteLine("\n=== PHASE III: Keystream Recovery ===");
        var recoveredKeystream = CryptoSystem.RecoverKeystream(plainText, cipherTextBits);
        Console.WriteLine("Keystream recovered successfully.");

        Console.WriteLine("\n=== PHASE IV: Correlation Attack ===");
        var attacker = new AttackService();

        var swCorr = Stopwatch.StartNew();
        var resultCorr = attacker.CorrelationAttack(recoveredKeystream);
        swCorr.Stop();

        Console.WriteLine("\n=== PHASE V: Verification ===");
        PrintVerification("X", keyX, resultCorr.StateX);
        PrintVerification("Y", keyY, resultCorr.StateY);
        PrintVerification("Z", keyZ, resultCorr.StateZ);

        generator.Reset(resultCorr.StateX, resultCorr.StateY, resultCorr.StateZ);

        var decryptedText = cryptoSystem.Decrypt(cipherTextBits);

        Console.WriteLine($"Decrypted Text: {decryptedText}");
        Console.WriteLine(decryptedText == plainText ? "SUCCESS: Attack successful." : "FAILURE: Decryption mismatch.");

        Console.WriteLine("\n=== Comparison: Brute Force ===");
        var swForce = Stopwatch.StartNew();
        var resultForce = attacker.BruteForceAttack(recoveredKeystream);
        swForce.Stop();

        var keysMatch = Enumerable.SequenceEqual(resultCorr.StateX, resultForce.StateX) &&
                        Enumerable.SequenceEqual(resultCorr.StateY, resultForce.StateY) &&
                        Enumerable.SequenceEqual(resultCorr.StateZ, resultForce.StateZ);

        Console.WriteLine($"Brute Force Result Matches Correlation: {keysMatch}");
        Console.WriteLine($"Correlation Attack Time: {swCorr.Elapsed.TotalMilliseconds:F4} ms");
        Console.WriteLine($"Brute Force Attack Time: {swForce.Elapsed.TotalMilliseconds:F4} ms");

        Console.WriteLine("\n=== PHASE VI: File I/O Demonstration ===");
        const string testFile = "secret_message.txt";
        const string encFile = "secret_message.enc";
        const string decFile = "secret_message_dec.txt";

        File.WriteAllText(testFile, "Top secret data for files or something.");

        generator.Reset(keyX, keyY, keyZ);
        cryptoSystem.EncryptFile(testFile, encFile);

        generator.Reset(keyX, keyY, keyZ);
        cryptoSystem.DecryptFile(encFile, decFile);

        Console.WriteLine($"Original File Content: {File.ReadAllText(testFile)}");
        Console.WriteLine($"Decrypted File Content: {File.ReadAllText(decFile)}");

        ExperimentRunner.RunExperiments();
    }

    private static int[] GenerateNonZeroState(int length, Random rnd)
    {
        while (true)
        {
            var val = rnd.Next(1, 1 << length);
            return BitUtils.IntToBinaryArray(val, length);
        }
    }

    private static void PrintVerification(string name, int[] original, int[] recovered)
    {
        var sOrg = string.Join("", original);
        var sRec = string.Join("", recovered);
        var status = sOrg == sRec ? "[OK]" : "[FAIL]";
        Console.WriteLine($"{name}: Orig={sOrg} Rec={sRec} {status}");
    }
}