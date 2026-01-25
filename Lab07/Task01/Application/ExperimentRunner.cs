namespace Task01.Application;

using System.Diagnostics;
using Domain.Core;
using Domain.Services;
using Shared;

public class ExperimentRunner(ITriviumCipher cipher)
{
    private const string TestKeyHex = "0123456789ABCDEF0123";
    private const string TestIvHex = "FEDCBA9876543210FEDC";

    public void RunExperiment1_Verification()
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
        Console.WriteLine();
    }

    public void RunExperiment2_IvReuse()
    {
        Console.WriteLine("--- Experiment 2: IV Reuse Attack ---");
        var key = TestKeyHex.HexToBits();
        var iv = TestIvHex.HexToBits();

        const string p1Str = "Tajna wiadomosc nr jeden!!";
        const string p2Str = "Inna sekretna informacja!!";

        var p1 = System.Text.Encoding.ASCII.GetBytes(p1Str);
        var p2 = System.Text.Encoding.ASCII.GetBytes(p2Str);

        var swEnc = Stopwatch.StartNew();
        cipher.Initialize(key, iv);
        var c1 = cipher.Encrypt(p1);

        cipher.Initialize(key, iv);
        var c2 = cipher.Encrypt(p2);
        swEnc.Stop();
        Console.WriteLine($"Encryption (C1, C2) took: {swEnc.Elapsed.TotalMicroseconds:F2} μs");

        var swXor = Stopwatch.StartNew();
        var xorCipher = new byte[c1.Length];
        for (var i = 0; i < c1.Length; i++) xorCipher[i] = (byte)(c1[i] ^ c2[i]);
        swXor.Stop();
        Console.WriteLine($"XOR elimination took: {swXor.Elapsed.TotalMicroseconds:F2} μs");

        int[] cribLengths = [2, 4, 8, 16];
        foreach (var len in cribLengths)
        {
            var swCrib = Stopwatch.StartNew();
            var crib = p1.Take(len).ToArray();
            var matches = 0;
            for (var i = 0; i < xorCipher.Length - crib.Length; i++)
            {
                var attempt = new byte[crib.Length];
                for (var j = 0; j < crib.Length; j++) attempt[j] = (byte)(xorCipher[i + j] ^ crib[j]);

                if (attempt.All(b => b is >= 32 and <= 126)) matches++;
            }
            swCrib.Stop();
            Console.WriteLine($"Length {len}: {matches} matches found in {swCrib.Elapsed.TotalMicroseconds:F2} μs");
        }
        Console.WriteLine();
    }

    public void RunExperiment3_RoundsAnalysis()
    {
        Console.WriteLine("--- Experiment 3: Rounds Analysis Performance ---");
        int[] roundsList = [0, 192, 288, 384, 480, 576, 768, 1152];
        var key = TestKeyHex.HexToBits();
        var iv = TestIvHex.HexToBits();

        Console.WriteLine($"{"Rounds",-8} | {"Warmup (μs)",-12} | {"Gen 1Mb (μs)",-12} | {"Throughput",-12}");

        foreach (var r in roundsList)
        {
            cipher.Initialize(key, iv, r);
            var warmupUs = ((TriviumCipher)cipher).LastWarmupTicks * 1_000_000.0 / Stopwatch.Frequency;

            cipher.GenerateKeystream(1_000_000);
            var genUs = ((TriviumCipher)cipher).LastGenerationTicks * 1_000_000.0 / Stopwatch.Frequency;
            var mbps = 1_000_000.0 / (genUs / 1_000_000.0) / 1_000_000.0;

            Console.WriteLine($"{r,-8} | {warmupUs,-12:F2} | {genUs,-12:F2} | {mbps,-12:F2} Mbps");
        }
        Console.WriteLine();
    }

    public void RunExperiment4_CubeAttack()
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

            Console.WriteLine($"Rounds: {r} | Found: {cubes.Count} | Accuracy: {(cubes.Count > 0 ? (double)correct / cubes.Count : 0):P1} | Offline: {swOffline.Elapsed.TotalMicroseconds:F2} μs");
        }
        Console.WriteLine();
    }

    public void RunExperiment5_Statistics()
    {
        Console.WriteLine("--- Experiment 5: Statistical Comparison ---");
        int[] rounds = [0, 288, 1152];
        var key = TestKeyHex.HexToBits();
        var iv = TestIvHex.HexToBits();

        foreach (var r in rounds)
        {
            Console.WriteLine($"Testing rounds: {r}");
            cipher.Initialize(key, iv, r);
            var stream = cipher.GenerateKeystream(100000);
            StatisticalTestService.RunTests(stream);
        }
    }

    private sealed class OracleTriviumWrapper(bool[] hiddenKey) : ITriviumCipher
    {
        private readonly TriviumCipher _internal = new();
        public void Initialize(bool[] key, bool[] iv, int warmupRounds = 1152) => _internal.Initialize(hiddenKey, iv, warmupRounds);
        public bool GenerateBit() => _internal.GenerateBit();
        public bool[] GenerateKeystream(int length) => _internal.GenerateKeystream(length);
        public byte[] Encrypt(byte[] plaintext) => _internal.Encrypt(plaintext);
        public byte[] Decrypt(byte[] ciphertext) => _internal.Decrypt(ciphertext);
        public (int OnesCount, double Balance) GetStateStatistics() => _internal.GetStateStatistics();
    }
}