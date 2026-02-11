using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Task01.Domain.Attack;
using Task01.Domain.Cryptography;
using Task01.Domain.Numeric;

namespace Task01.Application;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
public static class ExperimentRunner
{
    public static void RunAll()
    {
        RunExperiment1();
        RunExperiment2();
        RunExperiment3();
    }

    private static void RunExperiment1()
    {
        Console.WriteLine("\n========== Experiment 1: Known plaintext length ==========");
        const int stateBitLength = 100;
        var modulus = PrimeGenerator.FindPrimeNearPowerOfTwo(stateBitLength, 10);
        var a = RandomBigIntegerGenerator.Next(BigInteger.One, modulus);
        var b = RandomBigIntegerGenerator.Next(BigInteger.Zero, modulus);
        var s0 = RandomBigIntegerGenerator.Next(BigInteger.Zero, modulus);

        var generator = new LcgKeyStreamGenerator(a, b, modulus, s0, stateBitLength);
        IStreamCipher cipher = new StreamCipher();
        const string plaintext = AttackDemo.DefaultPlaintext;
        var ciphertextBits = cipher.Encrypt(plaintext, generator);

        var targetKnownBits = new[] { 200, 250, 300, 350 };

        const string headerFormat = "{0,10} {1,11} {2,8} {3,10} {4,15}";
        const string rowFormat = "{0,10} {1,11} {2,8} {3,10} {4,15}";

        Console.WriteLine(headerFormat, "TargetBits", "ActualBits", "Success", "gcd(mu,m)", "FailureType");

        foreach (var targetBits in targetKnownBits)
        {
            var knownChars = Math.Min(plaintext.Length, (targetBits + 7) / 8);
            var knownPlaintext = plaintext[..knownChars];
            IKnownPlaintextAttacker attacker = new LcgKnownPlaintextAttacker();
            var result = attacker.RecoverParameters(knownPlaintext, ciphertextBits, modulus, stateBitLength);
            var actualBits = knownPlaintext.Length * 8;
            var successText = result.Success ? "yes" : "no";
            var gcdText = result.GcdMuModulus.HasValue ? result.GcdMuModulus.Value.ToString() : "-";
            var failureType = string.Empty;

            if (!result.Success)
            {
                if (result.IsInsufficientKeystream)
                {
                    failureType = "insufficient-keystream";
                }
                else if (result.IsAmbiguousSolutions)
                {
                    failureType = "ambiguous";
                }
                else if (result.IsVerificationFailed)
                {
                    failureType = "verification-failed";
                }
                else
                {
                    failureType = "other";
                }
            }

            Console.WriteLine(rowFormat, targetBits, actualBits, successText, gcdText, failureType);
        }

        Console.WriteLine();
    }

    private static void RunExperiment2()
    {
        Console.WriteLine("========== Experiment 2: Modulus size and complexity ==========");
        var bitLengths = new[] { 50, 100, 128 };
        const int trialsPerSize = 3;
        IStreamCipher cipher = new StreamCipher();
        const string plaintext = AttackDemo.DefaultPlaintext;

        const string headerFormat = "{0,5} {1,10} {2,8} {3,10} {4,10} {5,10}";
        const string rowFormat = "{0,5} {1,10} {2,8} {3,10:F3} {4,10:F3} {5,10:F3}";

        foreach (var stateBitLength in bitLengths)
        {
            Console.WriteLine($"Modulus bit length n = {stateBitLength}");
            Console.WriteLine(headerFormat, "Trial", "KnownBits", "Success", "SetupUs", "EncryptUs", "AttackUs");

            long totalSetupTicks = 0;
            long totalEncryptTicks = 0;
            long totalAttackTicks = 0;
            var successfulAttacks = 0;

            for (var trial = 1; trial <= trialsPerSize; trial++)
            {
                var setupWatch = Stopwatch.StartNew();
                var modulus = PrimeGenerator.FindPrimeNearPowerOfTwo(stateBitLength, 10);
                var a = RandomBigIntegerGenerator.Next(BigInteger.One, modulus);
                var b = RandomBigIntegerGenerator.Next(BigInteger.Zero, modulus);
                var s0 = RandomBigIntegerGenerator.Next(BigInteger.Zero, modulus);
                setupWatch.Stop();
                var setupTicks = setupWatch.ElapsedTicks;

                var generator = new LcgKeyStreamGenerator(a, b, modulus, s0, stateBitLength);
                var encryptWatch = Stopwatch.StartNew();
                var ciphertextBits = cipher.Encrypt(plaintext, generator);
                encryptWatch.Stop();
                var encryptTicks = encryptWatch.ElapsedTicks;

                var knownBitsTarget = 3 * stateBitLength;
                var knownChars = Math.Min(plaintext.Length, (knownBitsTarget + 7) / 8);
                var knownPlaintext = plaintext[..knownChars];
                var actualKnownBits = knownPlaintext.Length * 8;

                IKnownPlaintextAttacker attacker = new LcgKnownPlaintextAttacker();
                var attackWatch = Stopwatch.StartNew();
                var result = attacker.RecoverParameters(knownPlaintext, ciphertextBits, modulus, stateBitLength);
                attackWatch.Stop();
                var attackTicks = attackWatch.ElapsedTicks;

                var successText = result.Success ? "yes" : "no";

                if (result.Success)
                {
                    successfulAttacks++;
                }

                totalSetupTicks += setupTicks;
                totalEncryptTicks += encryptTicks;
                totalAttackTicks += attackTicks;

                var setupUs = TicksToMicroseconds(setupTicks);
                var encryptUs = TicksToMicroseconds(encryptTicks);
                var attackUs = TicksToMicroseconds(attackTicks);

                Console.WriteLine(rowFormat, trial, actualKnownBits, successText, setupUs, encryptUs, attackUs);
            }

            var averageSetupUs = totalSetupTicks * 1_000_000.0 / Stopwatch.Frequency / trialsPerSize;
            var averageEncryptUs = totalEncryptTicks * 1_000_000.0 / Stopwatch.Frequency / trialsPerSize;
            var averageAttackUs = totalAttackTicks * 1_000_000.0 / Stopwatch.Frequency / trialsPerSize;

            Console.WriteLine($"Average setup time [us]: {averageSetupUs:F3}");
            Console.WriteLine($"Average encryption time [us]: {averageEncryptUs:F3}");
            Console.WriteLine($"Average attack time [us]: {averageAttackUs:F3}");
            Console.WriteLine($"Successful attacks: {successfulAttacks} / {trialsPerSize}");
            Console.WriteLine();
        }
    }

    private static double TicksToMicroseconds(long ticks)
    {
        return ticks * 1_000_000.0 / Stopwatch.Frequency;
    }

    private static void RunExperiment3()
    {
        Console.WriteLine("========== Experiment 3: Frequency of gcd(mu,m) ≠ 1 ==========");
        const int stateBitLength = 100;
        const int trials = 20;
        IStreamCipher cipher = new StreamCipher();
        const string plaintext = AttackDemo.DefaultPlaintext;
        var countGcdNotOne = 0;
        var countAmbiguous = 0;

        const string headerFormat = "{0,5} {1,8} {2,12} {3,10}";
        const string rowFormat = "{0,5} {1,8} {2,12} {3,10}";

        Console.WriteLine(headerFormat, "Trial", "Success", "gcd(mu,m)", "Ambiguous");

        for (var trial = 1; trial <= trials; trial++)
        {
            var modulus = PrimeGenerator.FindPrimeNearPowerOfTwo(stateBitLength, 10);
            var a = RandomBigIntegerGenerator.Next(BigInteger.One, modulus);
            var b = RandomBigIntegerGenerator.Next(BigInteger.Zero, modulus);
            var s0 = RandomBigIntegerGenerator.Next(BigInteger.Zero, modulus);

            var generator = new LcgKeyStreamGenerator(a, b, modulus, s0, stateBitLength);
            var ciphertextBits = cipher.Encrypt(plaintext, generator);

            const int knownBitsTarget = 3 * stateBitLength;
            var knownChars = Math.Min(plaintext.Length, (knownBitsTarget + 7) / 8);
            var knownPlaintext = plaintext[..knownChars];

            IKnownPlaintextAttacker attacker = new LcgKnownPlaintextAttacker();
            var result = attacker.RecoverParameters(knownPlaintext, ciphertextBits, modulus, stateBitLength);

            var gcdText = result.GcdMuModulus.HasValue ? result.GcdMuModulus.Value.ToString() : "-";
            var successText = result.Success ? "yes" : "no";
            var ambiguousText = result.IsAmbiguousSolutions ? "yes" : "no";

            if (result.GcdMuModulus.HasValue && result.GcdMuModulus.Value != BigInteger.One)
            {
                countGcdNotOne++;
            }

            if (result.IsAmbiguousSolutions)
            {
                countAmbiguous++;
            }

            Console.WriteLine(rowFormat, trial, successText, gcdText, ambiguousText);
        }

        var frequency = (double)countGcdNotOne / trials;

        Console.WriteLine();
        Console.WriteLine($"Trials: {trials}");
        Console.WriteLine($"Cases with gcd(mu, m) ≠ 1: {countGcdNotOne}");
        Console.WriteLine($"Relative frequency: {frequency:F3}");
        Console.WriteLine($"Ambiguous cases reported by the attacker: {countAmbiguous}");
        Console.WriteLine(
            "In ambiguous cases an additional state S4 would be required to uniquely determine the parameters.");
        Console.WriteLine();
    }
}