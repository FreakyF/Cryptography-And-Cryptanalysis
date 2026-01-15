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

        var testMsg = "TriviumTest"u8.ToArray();
        cipher.Initialize(key, iv);
        var enc = cipher.Encrypt(testMsg);

        cipher.Initialize(key, iv);
        var dec = cipher.Decrypt(enc);

        Console.WriteLine($"Decrypt Check: {testMsg.SequenceEqual(dec)}");
        Console.WriteLine();
    }

    public void RunExperiment2_IvReuse()
    {
        Console.WriteLine("--- Experiment 2: IV Reuse Attack ---");
        var key = TestKeyHex.HexToBits();
        var iv = TestIvHex.HexToBits();

        const string p1Str = "Secret message number one!!";
        const string p2Str = "Another secret message number two!!";

        var p1 = System.Text.Encoding.ASCII.GetBytes(p1Str);
        var p2 = System.Text.Encoding.ASCII.GetBytes(p2Str);

        cipher.Initialize(key, iv);
        var c1 = cipher.Encrypt(p1);

        cipher.Initialize(key, iv); 
        var c2 = cipher.Encrypt(p2);

        var xorCipher = new byte[c1.Length];
        for (var i = 0; i < c1.Length; i++) xorCipher[i] = (byte)(c1[i] ^ c2[i]);

        var xorPlain = new byte[p1.Length];
        for (var i = 0; i < p1.Length; i++) xorPlain[i] = (byte)(p1[i] ^ p2[i]);

        Console.WriteLine($"C1 ^ C2 == P1 ^ P2: {xorCipher.SequenceEqual(xorPlain)}");

        var recoveredP2 = new byte[p1.Length];
        for (var i = 0; i < p1.Length; i++) recoveredP2[i] = (byte)(xorCipher[i] ^ p1[i]);

        Console.WriteLine($"Recovered P2: {recoveredP2.ToAsciiString()}");

        Console.WriteLine("Crib Dragging 'secret':");
        var crib = "secret"u8.ToArray();
        for (var i = 0; i < xorCipher.Length - crib.Length; i++)
        {
            var attempt = new byte[crib.Length];
            for (var j = 0; j < crib.Length; j++) attempt[j] = (byte)(xorCipher[i + j] ^ crib[j]);

            if (attempt.All(b => b is >= 32 and <= 126))
            {
                Console.WriteLine($"Pos {i}: {attempt.ToAsciiString()}");
            }
        }

        Console.WriteLine();
    }

    public void RunExperiment3_RoundsAnalysis()
    {
        Console.WriteLine("--- Experiment 3: Rounds Analysis ---");
        int[] rounds = [0, 192, 288, 576, 1152];
        var key = TestKeyHex.HexToBits();
        var iv = TestIvHex.HexToBits();

        Console.WriteLine($"{"Rounds",-10} {"Ones",-10} {"Balance",-10} {"Time(ms)",-10}");

        foreach (var r in rounds)
        {
            var sw = Stopwatch.StartNew();
            cipher.Initialize(key, iv, r);
            sw.Stop();

            var stats = cipher.GetStateStatistics();
            Console.WriteLine(
                $"{r,-10} {stats.OnesCount,-10} {stats.Balance,-10:F3} {sw.Elapsed.TotalMilliseconds,-10:F2}");
        }

        Console.WriteLine();
    }

    public void RunExperiment4_CubeAttack()
    {
        Console.WriteLine("--- Experiment 4: Cube Attack (Reduced 288 rounds) ---");
        var rounds = 288;
        var attackService = new CubeAttackService(cipher);

        Console.WriteLine("Offline Phase: Finding cubes...");
        var cubes = attackService.FindLinearCubes(rounds);
        Console.WriteLine($"Found {cubes.Count} usable cubes.");

        Console.WriteLine("Online Phase: Recovering key bits...");
        var targetKey = TestKeyHex.HexToBits();

        var oracle = new OracleTriviumWrapper(targetKey);

        var recoveredKey = CubeAttackService.RecoverKey(cubes, oracle, rounds);

        var correct = 0;
        var total = 0;
        foreach (var (_, keyIdx) in cubes)
        {
            total++;
            if (recoveredKey[keyIdx] == targetKey[keyIdx]) correct++;
            Console.WriteLine($"Index {keyIdx}: Rec={recoveredKey[keyIdx]} Act={targetKey[keyIdx]}");
        }

        Console.WriteLine(total > 0
            ? $"Accuracy: {(double)correct / total:P2}"
            : "No linear cubes found.");

        Console.WriteLine();
    }

    public void RunExperiment5_Statistics()
    {
        Console.WriteLine("--- Experiment 5: Statistical Analysis ---");
        var key = TestKeyHex.HexToBits();
        var iv = TestIvHex.HexToBits();

        // ReSharper disable once RedundantArgumentDefaultValue
        cipher.Initialize(key, iv, 1152);
        var stream = cipher.GenerateKeystream(1_000_000);

        StatisticalTestService.RunTests(stream);
        Console.WriteLine();
    }

    private sealed class OracleTriviumWrapper(bool[] hiddenKey) : ITriviumCipher
    {
        private readonly TriviumCipher _internal = new();

        public void Initialize(bool[] key, bool[] iv, int warmupRounds = 1152)
        {
            _internal.Initialize(hiddenKey, iv, warmupRounds);
        }

        public bool GenerateBit() => _internal.GenerateBit();
        public bool[] GenerateKeystream(int length) => throw new NotImplementedException();
        public byte[] Encrypt(byte[] plaintext) => throw new NotImplementedException();
        public byte[] Decrypt(byte[] ciphertext) => throw new NotImplementedException();
        public (int OnesCount, double Balance) GetStateStatistics() => throw new NotImplementedException();
    }
}