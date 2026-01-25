using System.Diagnostics;
using System.Text;
using Task01.Domain.Core;
using Task01.Domain.Services;
using Task01.Shared;

namespace Task01.Application;

/// <summary>
///     Orchestrates and executes the various cryptographic experiments and attacks required for the laboratory task.
/// </summary>
/// <param name="cipher">The Trivium cipher implementation to be tested.</param>
public class ExperimentRunner(ITriviumCipher cipher)
{
    private const string TestKeyHex = "0123456789ABCDEF0123";
    private const string TestIvHex = "FEDCBA9876543210FEDC";

    /// <summary>
    ///     Experiment 1: Verifies the correctness of the Trivium implementation against known test vectors.
    ///     Also checks the involutive property (Dec(Enc(P)) == P).
    /// </summary>
    public void RunExperiment1Verification()
    {
        Console.WriteLine("--- Experiment 1: Verification ---");
        var key = new byte[10];
        var iv = new byte[10];

        cipher.Initialize(key, iv);
        var stream = cipher.GenerateKeystream(32);
        var hex = Convert.ToHexString(stream);

        // Known expected output for zero key/IV from Trivium specification
        const string expected = "FBE0BF265859051B517A2E4E239FC97F563203161907CF2DE7A8790FA1B2E9CD";

        Console.WriteLine($"Generated: {hex}");
        Console.WriteLine($"Expected:  {expected}");
        Console.WriteLine($"Match:     {hex.Equals(expected, StringComparison.OrdinalIgnoreCase)}");

        // Involutive check
        cipher.Initialize(key, iv);
        var testData = "Trivium Involutive Test"u8.ToArray();
        var encrypted = cipher.Encrypt(testData);

        cipher.Initialize(key, iv);
        var decrypted = cipher.Decrypt(encrypted);
        Console.WriteLine($"Involutive Check (Dec(Enc(P)) == P): {testData.SequenceEqual(decrypted)}");
        Console.WriteLine();
    }

    /// <summary>
    ///     Experiment 2: Demonstrates the "IV Reuse" attack (Two-Time Pad).
    ///     Shows that encrypting two different plaintexts with the same Key/IV allows an attacker
    ///     to recover the XOR difference of the plaintexts and potentially recover the plaintexts themselves using crib dragging.
    /// </summary>
    public void RunExperiment2IvReuse()
    {
        Console.WriteLine("--- Experiment 2: IV Reuse Attack ---");
        var key = Convert.FromHexString(TestKeyHex);
        var iv = Convert.FromHexString(TestIvHex);

        const string p1Str = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nSecret: 12345";
        const string p2Str = "HTTP/1.1 404 Not Found\r\nFrom: admin@local\r\n\r\nData: 67890";

        var p1 = Encoding.ASCII.GetBytes(p1Str);
        var p2 = Encoding.ASCII.GetBytes(p2Str);
        var minLen = Math.Min(p1.Length, p2.Length);

        // Encrypt both plaintexts with the SAME keystream
        var swEnc = Stopwatch.StartNew();
        cipher.Initialize(key, iv);
        var c1 = cipher.Encrypt(p1);
        cipher.Initialize(key, iv);
        var c2 = cipher.Encrypt(p2);
        swEnc.Stop();
        Console.WriteLine($"Encryption took: {swEnc.Elapsed.TotalMicroseconds:F2} μs");

        // XORing ciphertexts eliminates the keystream: C1 ^ C2 = (P1 ^ K) ^ (P2 ^ K) = P1 ^ P2
        Span<byte> xorCipher = stackalloc byte[minLen];
        for (var i = 0; i < minLen; i++)
        {
            xorCipher[i] = (byte)(c1[i] ^ c2[i]);
        }

        // Trivial recovery if we know P1
        Span<byte> recoveredP2 = stackalloc byte[minLen];
        for (var i = 0; i < minLen; i++)
        {
            recoveredP2[i] = (byte)(xorCipher[i] ^ p1[i]);
        }

        Console.WriteLine($"Recovered P2: {Encoding.ASCII.GetString(recoveredP2)}");

        // Crib dragging demonstration
        string[] cribs = ["HTTP", "Content-Type:", "Secret:", "200 OK"];
        foreach (var cribStr in cribs)
        {
            AnalyzeCrib(xorCipher, cribStr, minLen);
        }

        Console.WriteLine();
    }

    /// <summary>
    ///     Helper method for crib dragging analysis.
    /// </summary>
    private static void AnalyzeCrib(ReadOnlySpan<byte> xorCipher, string cribStr, int maxLen)
    {
        var sw = Stopwatch.StartNew();
        var cribBytes = Encoding.ASCII.GetBytes(cribStr);
        var matches = CountCribMatches(xorCipher, cribBytes, maxLen);
        sw.Stop();
        Console.WriteLine($"Crib '{cribStr}': {matches} matches in {sw.Elapsed.TotalMicroseconds:F2} μs");
    }

    private static int CountCribMatches(ReadOnlySpan<byte> xorCipher, ReadOnlySpan<byte> crib, int maxLen)
    {
        var matches = 0;
        var searchLimit = maxLen - crib.Length;

        for (var i = 0; i <= searchLimit; i++)
        {
            if (IsDecryptionSensible(xorCipher.Slice(i, crib.Length), crib))
            {
                matches++;
            }
        }

        return matches;
    }

    /// <summary>
    ///     Checks if XORing the crib with the ciphertext segment results in sensible ASCII text.
    /// </summary>
    private static bool IsDecryptionSensible(ReadOnlySpan<byte> xorSegment, ReadOnlySpan<byte> crib)
    {
        for (var j = 0; j < crib.Length; j++)
        {
            var val = (byte)(xorSegment[j] ^ crib[j]);
            // Check for printable ASCII or common whitespace
            if (val is not (>= 32 and <= 126 or 10 or 13))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Experiment 3: Analyzes the effect of reduced rounds on state statistics and performance.
    ///     Measures warmup time, keystream generation throughput, and statistical properties (Chi-Square)
    ///     for various round counts (0 to 1152).
    /// </summary>
    public void RunExperiment3RoundsAnalysis()
    {
        Console.WriteLine("--- Experiment 3: Rounds Analysis Performance & State Evolution ---");
        int[] roundsList = [0, 192, 288, 384, 480, 576, 768, 1152];
        var key = Convert.FromHexString(TestKeyHex);
        var iv = Convert.FromHexString(TestIvHex);

        Console.WriteLine(
            $"{"Rounds",-8} | {"Ones",-6} | {"Balance",-8} | {"Chi-Sq",-10} | {"Warmup (μs)",-12} | {"Throughput",-12}");

        foreach (var r in roundsList)
        {
            cipher.Initialize(key, iv, r);
            var stats = cipher.GetStateStatistics();
            var warmupUs = ((TriviumCipher)cipher).LastWarmupTicks * 1_000_000.0 / Stopwatch.Frequency;

            var stream10K = cipher.GenerateKeystream(1250);
            var chiSq = StatisticalTestService.CalculateChiSquare(stream10K);

            cipher.GenerateKeystream(125_000);
            var genUs = ((TriviumCipher)cipher).LastGenerationTicks * 1_000_000.0 / Stopwatch.Frequency;
            var mbps = 1_000_000.0 / genUs;

            Console.WriteLine(
                $"{r,-8} | {stats.OnesCount,-6} | {stats.Balance,-8:F3} | {chiSq,-10:F2} | {warmupUs,-12:F2} | {mbps,-12:F2} Mbps");
        }

        Console.WriteLine();
    }

    /// <summary>
    ///     Experiment 4: Performs a Cube Attack on reduced-round versions of Trivium.
    ///     Attempts to recover key bits by finding linear cubes in the offline phase and solving the system in the online phase.
    /// </summary>
    public void RunExperiment4CubeAttack()
    {
        Console.WriteLine("--- Experiment 4: Cube Attack (Reduced Versions) ---");
        int[] testRounds = [192, 288, 384, 480];
        var targetKey = TestKeyHex.HexToBits();

        foreach (var r in testRounds)
        {
            var attackService = new CubeAttackService(cipher);
            var swOffline = Stopwatch.StartNew();
            // Offline Phase
            var cubes = attackService.FindLinearCubes(r);
            swOffline.Stop();

            // Online Phase (using a wrapper to simulate an Oracle with a hidden key)
            var oracle = new OracleTriviumWrapper(targetKey);
            var recovered = CubeAttackService.RecoverKey(cubes, oracle, r);

            var correct = cubes.Count(item => recovered[item.KeyIndex] == targetKey[item.KeyIndex]);

            Console.WriteLine(
                $"Rounds: {r} | Found: {cubes.Count} | Accuracy: {(cubes.Count > 0 ? (double)correct / cubes.Count : 0):P1} | Offline: {swOffline.Elapsed.TotalMicroseconds:F2} μs");
        }
    }

    /// <summary>
    ///     Experiment 5: Runs extensive statistical tests (NIST-like) on keystreams generated with varying warmup rounds.
    /// </summary>
    public void RunExperiment5Statistics()
    {
        Console.WriteLine("--- Experiment 5: Statistical Comparison ---");
        int[] rounds = [0, 288, 1152];
        var key = Convert.FromHexString(TestKeyHex);
        var iv = Convert.FromHexString(TestIvHex);

        foreach (var r in rounds)
        {
            Console.WriteLine($"Testing rounds: {r}");
            cipher.Initialize(key, iv, r);
            var stream = cipher.GenerateKeystream(125_000); // ~1 Mbit
            StatisticalTestService.RunTests(stream);
        }
    }

    /// <summary>
    ///     Experiment 6: Saturation/Stress Test.
    ///     Generates 1 Gigabit of keystream to benchmark sustained throughput and check for memory stability.
    /// </summary>
    public void RunExperiment6HighVolumeThroughput()
    {
        Console.WriteLine("--- Experiment 6: 1 Billion Bits Saturation Test (Fixed) ---");
        var key = Convert.FromHexString(TestKeyHex);
        var iv = Convert.FromHexString(TestIvHex);

        const int totalBits = 1_000_000_000;
        const int totalBytes = totalBits / 8;

        Console.WriteLine($"Scale: {totalBits:N0} bits (1 Gbit)");

        cipher.Initialize(key, iv);
        var sw = Stopwatch.StartNew();
        // Generate 125MB of keystream
        _ = cipher.GenerateKeystream(totalBytes);
        sw.Stop();

        var ksMbps = totalBits / sw.Elapsed.TotalSeconds / 1_000_000.0;
        Console.WriteLine(
            $"Keystream (byte[])   | Time: {sw.Elapsed.TotalMilliseconds:F2} ms | Speed: {ksMbps:F2} Mbps");

        var plaintext = GC.AllocateUninitializedArray<byte>(totalBytes);
        new Random(42).NextBytes(plaintext);

        var originalCopy = new byte[8];
        Array.Copy(plaintext, originalCopy, 8);

        cipher.Initialize(key, iv);
        sw.Restart();

        // Encrypt 125MB in-place
        cipher.Encrypt(plaintext);

        sw.Stop();

        var encMbps = totalBits / sw.Elapsed.TotalSeconds / 1_000_000.0;
        Console.WriteLine($"Encryption (byte[])  | Speed: {encMbps / 1024.0:F2} Gbps");

        var hasChanged = BitConverter.ToUInt64(plaintext, 0) != BitConverter.ToUInt64(originalCopy, 0);
        Console.WriteLine($"Integrity Check: {hasChanged}");
        Console.WriteLine();
    }

    /// <summary>
    ///     A wrapper around the <see cref="TriviumCipher"/> that acts as an Oracle with a hidden embedded key.
    ///     Used in the online phase of the Cube Attack.
    /// </summary>
    /// <param name="hiddenKey">The secret key hidden within the oracle.</param>
    private sealed class OracleTriviumWrapper(bool[] hiddenKey) : ITriviumCipher
    {
        private readonly TriviumCipher _internal = new();

        public void Initialize(byte[] key, byte[] iv, int warmupRounds = 1152)
        {
            // Ignores the provided 'key' argument and uses 'hiddenKey' instead
            _internal.Initialize(BitsToBytes(hiddenKey), iv, warmupRounds);
        }

        public bool GenerateBit()
        {
            return _internal.GenerateBit();
        }

        public byte[] GenerateKeystream(int length)
        {
            return _internal.GenerateKeystream(length);
        }

        public byte[] Encrypt(byte[] plaintext)
        {
            return _internal.Encrypt(plaintext);
        }

        public byte[] Decrypt(byte[] ciphertext)
        {
            return _internal.Decrypt(ciphertext);
        }

        public (int OnesCount, double Balance) GetStateStatistics()
        {
            return _internal.GetStateStatistics();
        }

        private static byte[] BitsToBytes(bool[] bits)
        {
            var bytes = new byte[10];
            for (var i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    bytes[i / 8] |= (byte)(1 << (i % 8));
                }
            }

            return bytes;
        }
    }
}
