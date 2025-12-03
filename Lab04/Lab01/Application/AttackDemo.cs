using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Lab01.Domain.Attack;
using Lab01.Domain.Cryptography;
using Lab01.Domain.Numeric;

namespace Lab01.Application;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
public static class AttackDemo
{
    internal const string DefaultPlaintext =
        "Linear congruential generators are not secure for cryptographic purposes. This demonstration shows how a known-plaintext attack fully recovers the key stream and breaks the cipher.";

    public static void Run()
    {
        const int stateBitLength = 100;
        var modulus = PrimeGenerator.FindPrimeNearPowerOfTwo(stateBitLength, 10);
        var a = RandomBigIntegerGenerator.Next(BigInteger.One, modulus);
        var b = RandomBigIntegerGenerator.Next(BigInteger.Zero, modulus);
        var s0 = RandomBigIntegerGenerator.Next(BigInteger.Zero, modulus);

        Console.WriteLine("Phase I: Cipher parameter generation");
        Console.WriteLine($"State bit length n: {stateBitLength}");
        Console.WriteLine($"Modulus m: {modulus}");
        Console.WriteLine($"Parameter A: {a}");
        Console.WriteLine($"Parameter B: {b}");
        Console.WriteLine($"Seed S0: {s0}");
        Console.WriteLine();

        var generator = new LcgKeyStreamGenerator(a, b, modulus, s0, stateBitLength);
        IStreamCipher cipher = new StreamCipher();
        var ciphertextBits = cipher.Encrypt(DefaultPlaintext, generator);

        Console.WriteLine("Phase II: Message encryption");
        Console.WriteLine($"Plaintext length in characters: {DefaultPlaintext.Length}");
        Console.WriteLine($"Ciphertext first 100 bits: {FormatFirstBits(ciphertextBits, 100)}");
        Console.WriteLine();

        const int knownPlaintextLength = 38;
        var knownPlaintext = DefaultPlaintext[..knownPlaintextLength];
        const int knownBits = knownPlaintextLength * 8;
        const int requiredBits = 3 * stateBitLength;

        Console.WriteLine("Phase III: Cryptanalytic known-plaintext attack");
        Console.WriteLine($"Known plaintext length in characters: {knownPlaintextLength}");
        Console.WriteLine($"Known plaintext length in bits: {knownBits}");
        Console.WriteLine($"Required bits for three states (3n): {requiredBits}");
        Console.WriteLine($"Assumption: the attacker knows the first {knownBits} bits of the keystream.");
        Console.WriteLine($"Known plaintext X_k: {knownPlaintext}");
        Console.WriteLine();

        IKnownPlaintextAttacker attacker = new LcgKnownPlaintextAttacker();
        var attackResult = attacker.RecoverParameters(knownPlaintext, ciphertextBits, modulus, stateBitLength);

        if (!attackResult.Success)
        {
            Console.WriteLine("Phase III result: attack failed.");
            Console.WriteLine($"Reason: {attackResult.FailureReason}");

            if (attackResult.IsInsufficientKeystream)
            {
                if (attackResult is { RequiredKeystreamBits: not null, ActualKeystreamBits: not null })
                {
                    Console.WriteLine($"Required keystream bits: {attackResult.RequiredKeystreamBits.Value}");
                    Console.WriteLine($"Available keystream bits: {attackResult.ActualKeystreamBits.Value}");
                }

                Console.WriteLine(
                    "Explanation: the known plaintext fragment does not cover three full generator states.");
                Console.WriteLine(
                    "Suggested action: increase the length of the known plaintext so that at least 3n bits are known.");
            }
            else if (attackResult.IsAmbiguousSolutions)
            {
                if (attackResult.GcdMuModulus.HasValue)
                {
                    Console.WriteLine($"Details: gcd(mu, m) = {attackResult.GcdMuModulus.Value} ≠ 1.");
                }

                Console.WriteLine("Explanation: multiple pairs (A, B) satisfy the equations for the observed states.");
                Console.WriteLine(
                    "Suggested action: use at least one additional state S4 (≥ 4n known keystream bits) or collect more known plaintext.");
            }
            else if (attackResult.IsVerificationFailed)
            {
                Console.WriteLine(
                    "Explanation: the recovered parameters do not reproduce the third generator state S3.");
                Console.WriteLine(
                    "Suggested action: verify the implementation and repeat the attack, preferably with more known plaintext bits.");
            }

            return;
        }

        Console.WriteLine("Phase III result: parameters recovered successfully.");
        Console.WriteLine($"Recovered A*: {attackResult.A}");
        Console.WriteLine($"Recovered B*: {attackResult.B}");

        if (attackResult.GcdMuModulus.HasValue)
        {
            Console.WriteLine($"gcd(mu, m): {attackResult.GcdMuModulus.Value}");
            if (attackResult.GcdMuModulus.Value == BigInteger.One)
            {
                Console.WriteLine("The solution is unique because gcd(mu, m) = 1.");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Phase IV: Verification of recovered parameters");
        Console.WriteLine($"A matches original: {attackResult.A == a}");
        Console.WriteLine($"B matches original: {attackResult.B == b}");
        Console.WriteLine();

        var recoveredStream = KeyStreamRecovery.RecoverFromKnownPlaintext(knownPlaintext, ciphertextBits);
        var s1Bits = BitConversion.Slice(recoveredStream, 0, stateBitLength);
        var s1 = BitConversion.BitsToBigInteger(s1Bits);
        var aInverse = ModularArithmetic.ModInverse(attackResult.A, modulus);

        if (aInverse == null)
        {
            Console.WriteLine("Phase V: Recovery of initial state");
            Console.WriteLine("Could not compute modular inverse of A when recovering S0.");
            Console.WriteLine("Suggested action: verify that the modulus is prime and repeat the experiment.");
            return;
        }

        var recoveredS0 = ModularArithmetic.NormalizeMod(s1 - attackResult.B, modulus);
        recoveredS0 = ModularArithmetic.NormalizeMod(recoveredS0 * aInverse.Value, modulus);

        Console.WriteLine("Phase V: Recovery of initial state");
        Console.WriteLine($"Recovered S0*: {recoveredS0}");
        Console.WriteLine($"Seed matches original: {recoveredS0 == s0}");
        Console.WriteLine();

        var reconstructedGenerator = new LcgKeyStreamGenerator(
            attackResult.A,
            attackResult.B,
            modulus,
            recoveredS0,
            stateBitLength);

        var decryptedPlaintext = cipher.Decrypt(ciphertextBits, recoveredS0, reconstructedGenerator);

        Console.WriteLine("Phase VI: Decryption of full message");
        Console.WriteLine("Decrypted plaintext:");
        Console.WriteLine(decryptedPlaintext);
        Console.WriteLine();
        Console.WriteLine($"Decryption successful (X* = X): {decryptedPlaintext == DefaultPlaintext}");
    }

    private static string FormatFirstBits(bool[] bits, int count)
    {
        var length = Math.Min(bits.Length, count);
        var chars = new char[length];

        for (var i = 0; i < length; i++)
        {
            chars[i] = bits[i] ? '1' : '0';
        }

        return new string(chars);
    }
}