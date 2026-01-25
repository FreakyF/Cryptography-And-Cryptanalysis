namespace Task01.Application;

using System.Diagnostics;
using Domain.Core;
using Domain.Services;
using Shared;

public class ExperimentRunner(ITriviumCipher cipher)
{
    private const string TestKeyHex = "0123456789ABCDEF0123";
    private const string TestIvHex = "FEDCBA9876543210FEDC";

    public void RunExperiment1Verification()
    {
        Console.WriteLine("--- Experiment 1: Verification ---");
        var key = new bool[80];
        var iv = new bool[80];

        cipher.Initialize(key, iv);
        var stream = cipher.GenerateKeystream(256);
        var hex = stream.BitsToHex();

        const string expected = "FBE0BF265859051B517A2E4E239FC97F563203161907CF2DE7A8790FA1B2E9CD";

        Console.WriteLine($"Generated: {hex}");
        Console.WriteLine($"Expected:  {expected}");
        Console.WriteLine($"Match:     {hex.Equals(expected, StringComparison.OrdinalIgnoreCase)}");

        cipher.Initialize(key, iv);
        var testData = "Trivium Involutive Test"u8.ToArray();
        var encrypted = cipher.Encrypt(testData);

        cipher.Initialize(key, iv);
        var decrypted = cipher.Decrypt(encrypted);
        Console.WriteLine($"Involutive Check (Dec(Enc(P)) == P): {testData.SequenceEqual(decrypted)}");
        Console.WriteLine();
    }

    public void RunExperiment2IvReuse()
    {
        Console.WriteLine("--- Experiment 2: IV Reuse Attack ---");
        var key = TestKeyHex.HexToBits();
        var iv = TestIvHex.HexToBits();

        const string p1Str = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nSecret: 12345";
        const string p2Str = "HTTP/1.1 404 Not Found\r\nFrom: admin@local\r\n\r\nData: 67890";

        var p1 = System.Text.Encoding.ASCII.GetBytes(p1Str);
        var p2 = System.Text.Encoding.ASCII.GetBytes(p2Str);
        var minLen = Math.Min(p1.Length, p2.Length);

        var swEnc = Stopwatch.StartNew();
        cipher.Initialize(key, iv);
        var c1 = cipher.Encrypt(p1);
        cipher.Initialize(key, iv);
        var c2 = cipher.Encrypt(p2);
        swEnc.Stop();
        Console.WriteLine($"Encryption took: {swEnc.Elapsed.TotalMicroseconds:F2} μs");

        Span<byte> xorCipher = stackalloc byte[minLen];
        for (var i = 0; i < minLen; i++) xorCipher[i] = (byte)(c1[i] ^ c2[i]);

        Span<byte> recoveredP2 = stackalloc byte[minLen];
        for (var i = 0; i < minLen; i++) recoveredP2[i] = (byte)(xorCipher[i] ^ p1[i]);
        Console.WriteLine($"Recovered P2: {recoveredP2.ToArray().ToAsciiString()}");

        string[] cribs = ["HTTP", "Content-Type:", "Secret:", "200 OK"];
        foreach (var cribStr in cribs)
        {
            AnalyzeCrib(xorCipher, cribStr, minLen);
        }

        Console.WriteLine();
    }

    private static void AnalyzeCrib(ReadOnlySpan<byte> xorCipher, string cribStr, int maxLen)
    {
        var sw = Stopwatch.StartNew();
        var cribBytes = System.Text.Encoding.ASCII.GetBytes(cribStr);
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

    private static bool IsDecryptionSensible(ReadOnlySpan<byte> xorSegment, ReadOnlySpan<byte> crib)
    {
        for (var j = 0; j < crib.Length; j++)
        {
            var val = (byte)(xorSegment[j] ^ crib[j]);
            if (val is not (>= 32 and <= 126 or 10 or 13)) return false;
        }

        return true;
    }

    public void RunExperiment3RoundsAnalysis()
    {
        Console.WriteLine("--- Experiment 3: Rounds Analysis Performance & State Evolution ---");
        int[] roundsList = [0, 192, 288, 384, 480, 576, 768, 1152];
        var key = TestKeyHex.HexToBits();
        var iv = TestIvHex.HexToBits();

        Console.WriteLine(
            $"{"Rounds",-8} | {"Ones",-6} | {"Balance",-8} | {"Chi-Sq",-10} | {"Warmup (μs)",-12} | {"Throughput",-12}");

        foreach (var r in roundsList)
        {
            cipher.Initialize(key, iv, r);
            var stats = cipher.GetStateStatistics();
            var warmupUs = ((TriviumCipher)cipher).LastWarmupTicks * 1_000_000.0 / Stopwatch.Frequency;

            var stream10K = cipher.GenerateKeystream(10000);
            var chiSq = StatisticalTestService.CalculateChiSquare(stream10K);

            cipher.GenerateKeystream(1_000_000);
            var genUs = ((TriviumCipher)cipher).LastGenerationTicks * 1_000_000.0 / Stopwatch.Frequency;
            var mbps = 1_000_000.0 / (genUs / 1_000_000.0) / 1_000_000.0;

            Console.WriteLine(
                $"{r,-8} | {stats.OnesCount,-6} | {stats.Balance,-8:F3} | {chiSq,-10:F2} | {warmupUs,-12:F2} | {mbps,-12:F2} Mbps");
        }

        Console.WriteLine();
    }

    public void RunExperiment4CubeAttack()
    {
        Console.WriteLine("--- Experiment 4: Cube Attack (Reduced Versions) ---");
        int[] testRounds = [192, 288, 384, 480];
        var targetKey = TestKeyHex.HexToBits();

        foreach (var r in testRounds)
        {
            var attackService = new CubeAttackService(cipher);
            var swOffline = Stopwatch.StartNew();
            var cubes = attackService.FindLinearCubes(r);
            swOffline.Stop();

            var oracle = new OracleTriviumWrapper(targetKey);
            var recovered = CubeAttackService.RecoverKey(cubes, oracle, r);

            var correct = cubes.Count(item => recovered[item.KeyIndex] == targetKey[item.KeyIndex]);

            Console.WriteLine(
                $"Rounds: {r} | Found: {cubes.Count} | Accuracy: {(cubes.Count > 0 ? (double)correct / cubes.Count : 0):P1} | Offline: {swOffline.Elapsed.TotalMicroseconds:F2} μs");
        }

        Console.WriteLine();
    }

    public void RunExperiment5Statistics()
    {
        Console.WriteLine("--- Experiment 5: Statistical Comparison ---");
        int[] rounds = [0, 288, 1152];
        var key = TestKeyHex.HexToBits();
        var iv = TestIvHex.HexToBits();

        foreach (var r in rounds)
        {
            Console.WriteLine($"Testing rounds: {r}");
            cipher.Initialize(key, iv, r);
            var stream = cipher.GenerateKeystream(1000000);
            StatisticalTestService.RunTests(stream);
        }
    }

    private sealed class OracleTriviumWrapper(bool[] hiddenKey) : ITriviumCipher
    {
        private readonly TriviumCipher _internal = new();

        public void Initialize(bool[] key, bool[] iv, int warmupRounds = 1152) =>
            _internal.Initialize(hiddenKey, iv, warmupRounds);

        public bool GenerateBit() => _internal.GenerateBit();
        public bool[] GenerateKeystream(int length) => _internal.GenerateKeystream(length);
        public byte[] Encrypt(byte[] plaintext) => _internal.Encrypt(plaintext);
        public byte[] Decrypt(byte[] ciphertext) => _internal.Decrypt(ciphertext);
        public (int OnesCount, double Balance) GetStateStatistics() => _internal.GetStateStatistics();
    }
}