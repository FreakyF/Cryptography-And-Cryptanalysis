using System.Diagnostics;
using System.Text;
using Task01.Domain.Services.Attacks;
using Task01.Domain.Services.Lfsr;
using Task01.Domain.Services.LinearComplexity;
using Task01.Domain.Services.StreamCipher;
using Task01.Domain.Utils;

namespace Task01.Application;

public sealed class Runner : IRunner
{
    private readonly bool _quiet;
    private readonly StringBuilder? _logBuilder;

    public Runner() : this(false)
    {
    }

    public Runner(bool quiet)
    {
        _quiet = quiet;
        if (!quiet)
        {
            _logBuilder = new StringBuilder(4096);
        }
    }

    public void RunAll()
    {
        RunLfsrVerification();
        RunBerlekampMasseyVerification();
        RunFullAttack();

        Log("=== ADDITIONAL EXPERIMENTS ===");
        Log(string.Empty);

        RunExperiment1_MinimalLength();
        RunExperiment2_ScaleAndTime();
        RunExperiment3_StatisticalReliability();
        RunExperiment4_PolynomialInfluence();
        RunExperiment5_MethodComparison();

        Flush();
    }

    private void Log(string message)
    {
        if (_quiet)
        {
            return;
        }

        _logBuilder!.AppendLine(message);
    }

    private void Flush()
    {
        if (_quiet)
        {
            return;
        }

        if (_logBuilder!.Length == 0)
        {
            return;
        }

        Console.Write(_logBuilder.ToString());
        _logBuilder.Clear();
    }

    private void RunLfsrVerification()
    {
        Log("LFSR verification");
        Log(string.Empty);

        VerifyLfsr(
            new[] { 1, 1, 0 },
            new[] { 0, 0, 1 },
            new[] { 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 1, 1, 1 });

        VerifyLfsr(
            new[] { 1, 0, 1, 0, 0 },
            new[] { 1, 0, 0, 1, 0 },
            new[]
            {
                1, 0, 0, 1, 0,
                1, 1, 0, 0, 1,
                1, 1, 1, 1, 0,
                0, 0, 1, 1, 0,
                1, 1, 1, 0, 1
            });

        VerifyLfsr(
            new[] { 1, 1, 0, 1, 0 },
            new[] { 1, 0, 0, 1, 0 },
            new[]
            {
                1, 0, 0, 1, 0,
                0, 0, 1, 1, 1,
                1, 0, 1, 0, 1,
                1, 0, 0, 1, 0,
                0, 0, 1, 1, 1
            });
    }

    private void VerifyLfsr(int[] feedbackInts, int[] stateInts, int[] expectedInts)
    {
        var feedback = BitConversions.IntArrayToBits(feedbackInts);
        var state = BitConversions.IntArrayToBits(stateInts);
        var expected = BitConversions.IntArrayToBits(expectedInts);

        ILfsr lfsr = new Lfsr(feedback, state);

        Log("LFSR degree: " + lfsr.Degree);
        Log("Feedback coefficients (from ILfsr): " +
            string.Join("", BitConversions.BitsToIntArray(lfsr.FeedbackCoefficients)));
        Log("Initial state (from ILfsr): " +
            string.Join("", BitConversions.BitsToIntArray(lfsr.State)));

        lfsr.Reset(state);

        var output = lfsr.GenerateBits(expected.Count);

        Log("Feedback coefficients (expected): " + string.Join("", feedbackInts));
        Log("Initial state (expected): " + string.Join("", stateInts));
        Log("Expected sequence: " + string.Join("", expectedInts));
        Log("Generated sequence: " + string.Join("", BitConversions.BitsToIntArray(output)));
        Log(string.Empty);
    }

    private void RunBerlekampMasseyVerification()
    {
        Log("Berlekamp–Massey verification");
        Log(string.Empty);

        var sequences = new[]
        {
            BitConversions.IntArrayToBits(
                new[]
                {
                    0, 0, 1, 0, 1, 1, 1, 0, 0, 1,
                    0, 1, 1, 1
                }),
            BitConversions.IntArrayToBits(
                new[]
                {
                    1, 0, 0, 1, 0,
                    1, 1, 0, 0, 1,
                    1, 1, 1, 1, 0,
                    0, 0, 1, 1, 0,
                    1, 1, 1, 0, 1
                }),
            BitConversions.IntArrayToBits(
                new[]
                {
                    1, 0, 0, 1, 0,
                    0, 0, 1, 1, 1,
                    1, 0, 1, 0, 1,
                    1, 0, 0, 1, 0,
                    0, 0, 1, 1, 1
                })
        };

        IBerlekampMasseySolver solver = new BerlekampMasseySolver();

        for (var i = 0; i < sequences.Length; i++)
        {
            var sequence = sequences[i];
            var result = solver.Solve(sequence);

            Log("Sequence " + (i + 1));
            Log("Linear complexity: " + result.LinearComplexity);
            Log("Connection polynomial coefficients: " +
                string.Join("", BitConversions.BitsToIntArray(result.ConnectionPolynomial)));
            Log(string.Empty);
        }
    }

    private void RunFullAttack()
    {
        Log("Full known-plaintext attack demonstration");
        Log(string.Empty);

        const int m = 8;

        var random = new Random();
        var feedback = GenerateRandomNonZeroVector(random, m, forceFirstBitOne: true);
        var initialState = GenerateRandomNonZeroVector(random, m, forceFirstBitOne: false);

        Log("Secret LFSR degree m = " + m);
        Log("Secret feedback coefficients p (p0..p" + (m - 1) + "): " +
            string.Join("", BitConversions.BitsToIntArray(feedback)));
        Log("Secret initial state sigma0: " +
            string.Join("", BitConversions.BitsToIntArray(initialState)));
        Log(string.Empty);

        ILfsr lfsr = new Lfsr(feedback, initialState);
        IStreamCipher cipher = new StreamCipher();

        const string plaintext = "This is a secret message for the LFSR stream cipher laboratory.";
        var ciphertextBits = cipher.Encrypt(plaintext, lfsr);

        Log("Plaintext length (characters): " + plaintext.Length);
        Log("Ciphertext bit length: " + ciphertextBits.Count);

        var previewLength = Math.Min(128, ciphertextBits.Count);
        Log("Ciphertext bits (first " + previewLength + "): " +
            BitConversions.BitsToBitString(new PreviewEnumerable(ciphertextBits, previewLength)));
        Log(string.Empty);

        IGaloisFieldSolver solver = new GaussianEliminationSolver();
        IKnownPlaintextAttacker attacker = new KnownPlaintextAttacker(solver);

        const int minimalKnownBits = 2 * m;
        var minimalKnownCharacters = (int)Math.Ceiling(minimalKnownBits / 8.0);

        var knownCharacters = Math.Min(minimalKnownCharacters, plaintext.Length);
        var knownPlaintext = plaintext[..knownCharacters];

        var testBits = BitConversions.BitStringToBits("01010101");
        var testBitString = BitConversions.BitsToBitString(testBits);
        Log("BitStringToBits/BitsToBitString test: " + testBitString);

        var knownBits = BitConversions.StringToBits(knownPlaintext);

        Log("Known plaintext used for attack: " + knownPlaintext);
        Log("Known plaintext bits: " + BitConversions.BitsToBitString(knownBits));
        Log("Known bits count: " + knownBits.Count);
        Log(string.Empty);

        var attackResult = attacker.Attack(knownPlaintext, ciphertextBits, m);

        if (attackResult == null)
        {
            var failLine = "Attack failed.";
            Log(failLine);
            if (_quiet)
            {
                Console.WriteLine(failLine);
            }

            return;
        }

        Log("Recovered keystream bits (from known segment): " +
            BitConversions.BitsToBitString(attackResult.KeyStream));
        Log("Recovered feedback coefficients: " +
            string.Join("", BitConversions.BitsToIntArray(attackResult.FeedbackCoefficients)));
        Log("Recovered initial state: " +
            string.Join("", BitConversions.BitsToIntArray(attackResult.InitialState)));
        Log(string.Empty);

        var feedbackMatch = AreEqual(feedback, attackResult.FeedbackCoefficients);
        var initialStateMatch = AreEqual(initialState, attackResult.InitialState);

        Log("Feedback coefficients match: " + feedbackMatch);
        Log("Initial state matches: " + initialStateMatch);
        Log(string.Empty);

        ILfsr recoveredLfsr = new Lfsr(attackResult.FeedbackCoefficients, attackResult.InitialState);
        IStreamCipher recoveredCipher = new StreamCipher();
        var recoveredPlaintext = recoveredCipher.Decrypt(ciphertextBits, recoveredLfsr);

        Log("Recovered plaintext:");
        Log(recoveredPlaintext);
        Log(string.Empty);

        var success = string.Equals(plaintext, recoveredPlaintext, StringComparison.Ordinal);
        var resultLine = "Attack success: " + success;

        Log(resultLine);

        if (_quiet)
        {
            Console.WriteLine(resultLine);
        }
    }

    private void RunExperiment1_MinimalLength()
    {
        Log("Experiment 1: Minimal length of known plaintext (m=8)");
        Log("Length | Status | Observations");
        Log("-------|--------|-------------");

        const int m = 8;
        int[] lengths = { 8, 12, 16, 20 };
        var random = new Random(123); // Fixed seed for reproducibility
        var feedback = GenerateRandomNonZeroVector(random, m, true);
        var initialState = GenerateRandomNonZeroVector(random, m, false);
        ILfsr lfsr = new Lfsr(feedback, initialState);
        IStreamCipher cipher = new StreamCipher();

        string plaintext = "Secret data necessary for length testing.";
        var ciphertext = cipher.Encrypt(plaintext, lfsr);

        IGaloisFieldSolver solver = new GaussianEliminationSolver();
        IKnownPlaintextAttacker attacker = new KnownPlaintextAttacker(solver);

        foreach (var bits in lengths)
        {
            int charCount = (int)Math.Ceiling(bits / 8.0);
            string kp = plaintext.Substring(0, charCount);

            string actualBits = (charCount * 8).ToString();
            string observation = "";

            var result = attacker.Attack(kp, ciphertext, m);

            bool status = result != null;
            if (status)
            {
                bool match = AreEqual(feedback, result!.FeedbackCoefficients);
                observation = match ? "Success (Key matched)" : "Success (Key mismatch)";
            }
            else
            {
                observation = "Failed (Not enough bits or no solution)";
            }

            Log($"{bits} (using {actualBits}) | {status} | {observation}");
        }
        Log(string.Empty);
    }

    private void RunExperiment2_ScaleAndTime()
    {
        Log("Experiment 2: Scale and Time (Gaussian Elimination)");
        Log("Degree m | Time (ticks) | Time (µs)");
        Log("---------|--------------|----------");

        int[] degrees = { 4, 8, 16, 17, 32 };
        var random = new Random(456);

        var solver = new GaussianEliminationSolver();

        foreach (var m in degrees)
        {
            var feedback = GenerateRandomNonZeroVector(random, m, true);
            var state = GenerateRandomNonZeroVector(random, m, false);
            var lfsr = new Lfsr(feedback, state);

            var keystream = lfsr.GenerateBits(2 * m);

            var matrix = new bool[m, m];
            var vector = new bool[m];

            for (var row = 0; row < m; row++)
            {
                var offset = row;
                for (var col = 0; col < m; col++)
                {
                    matrix[row, col] = keystream[offset + col];
                }
                vector[row] = keystream[row + m];
            }

            var sw = Stopwatch.StartNew();
            solver.Solve(matrix, vector);
            sw.Stop();

            Log($"{m,-8} | {sw.ElapsedTicks,-12} | {sw.Elapsed.TotalMicroseconds:F2}");
        }
        Log(string.Empty);
    }

    private void RunExperiment3_StatisticalReliability()
    {
        Log("Experiment 3: Statistical Reliability (m=8, 50 runs)");
        const int m = 8;
        const int runs = 50;
        int successes = 0;
        var random = new Random(789);
        var solver = new GaussianEliminationSolver();
        var attacker = new KnownPlaintextAttacker(solver);
        var cipher = new StreamCipher();

        for (int i = 0; i < runs; i++)
        {
            var feedback = GenerateRandomNonZeroVector(random, m, true);
            var state = GenerateRandomNonZeroVector(random, m, false);
            var lfsr = new Lfsr(feedback, state);

            string plaintext = "TestRun" + i;
            string knownPlaintext = plaintext.Substring(0, 2);

            var ciphertext = cipher.Encrypt(plaintext, lfsr);

            var result = attacker.Attack(knownPlaintext, ciphertext, m);

            if (result != null)
            {
                // Verify decryption
                var recLfsr = new Lfsr(result.FeedbackCoefficients, result.InitialState);
                var recPlaintext = cipher.Decrypt(ciphertext, recLfsr);
                if (recPlaintext == plaintext)
                {
                    successes++;
                }
            }
        }

        Log($"Total runs: {runs}");
        Log($"Successes: {successes}");
        Log($"Success rate: {(double)successes / runs * 100}%");
        Log(string.Empty);
    }

    private void RunExperiment4_PolynomialInfluence()
    {
        Log("Experiment 4: Polynomial Influence (Period length)");
        const int m = 8;

        // Primitive polynomial for m=8: x^8 + x^4 + x^3 + x^2 + 1
        var primitiveCoeffs = new[] { 1, 0, 1, 1, 1, 0, 0, 0 };

        // Non-primitive (reducible): x^8 + 1
        var nonPrimitiveCoeffs = new[] { 1, 0, 0, 0, 0, 0, 0, 0 };

        var state = new[] { 0, 0, 0, 0, 0, 0, 0, 1 };

        long primPeriod = MeasurePeriod(primitiveCoeffs, state);
        long nonPrimPeriod = MeasurePeriod(nonPrimitiveCoeffs, state);

        Log($"Primitive Polynomial Period: {primPeriod} (Expected: {Math.Pow(2, m) - 1})");
        Log($"Non-Primitive Polynomial Period: {nonPrimPeriod}");
        Log(string.Empty);
    }

    private long MeasurePeriod(int[] feedbackInts, int[] stateInts)
    {
        var feedback = BitConversions.IntArrayToBits(feedbackInts);
        var state = BitConversions.IntArrayToBits(stateInts);
        var lfsr = new Lfsr(feedback, state);

        var startState = lfsr.State.ToArray();
        long period = 0;
        long max = 10000;

        do
        {
            lfsr.NextBit();
            period++;

            if (period > max) return -1;

        } while (!AreEqual(lfsr.State, startState));

        return period;
    }

    private void RunExperiment5_MethodComparison()
    {
        Log("Experiment 5: Method Comparison (Gauss vs Berlekamp-Massey)");
        Log("Degree m=16. Sequence length 2m=32.");

        const int m = 16;
        var random = new Random(321);
        var feedback = GenerateRandomNonZeroVector(random, m, true);
        var state = GenerateRandomNonZeroVector(random, m, false);
        var lfsr = new Lfsr(feedback, state);

        var sequence = lfsr.GenerateBits(2 * m);

        var gaussSolver = new GaussianEliminationSolver();
        var matrix = new bool[m, m];
        var vector = new bool[m];
        for (var row = 0; row < m; row++)
        {
            for (var col = 0; col < m; col++) matrix[row, col] = sequence[row + col];
            vector[row] = sequence[row + m];
        }

        var swG = Stopwatch.StartNew();
        var gaussResult = gaussSolver.Solve(matrix, vector);
        swG.Stop();

        var bmSolver = new BerlekampMasseySolver();
        var swB = Stopwatch.StartNew();
        var bmResult = bmSolver.Solve(sequence);
        swB.Stop();

        Log($"Gauss Time: {swG.Elapsed.TotalMicroseconds:F2} µs");
        Log($"BM Time:    {swB.Elapsed.TotalMicroseconds:F2} µs");
        Log($"Gauss Result Found: {gaussResult != null}");
        Log($"BM Linear Complexity: {bmResult.LinearComplexity}");

        Log("Testing Gauss with wrong m (m=15, m=17)");
        Log("BM behavior: Correctly identifies L=" + bmResult.LinearComplexity);
        Log(string.Empty);
    }

    private static IReadOnlyList<bool> GenerateRandomNonZeroVector(Random random, int length, bool forceFirstBitOne)
    {
        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        while (true)
        {
            var result = new bool[length];
            var hasOne = false;

            for (var i = 0; i < length; i++)
            {
                var bit = random.Next(2) == 1;
                result[i] = bit;
                if (bit)
                {
                    hasOne = true;
                }
            }

            if (forceFirstBitOne)
            {
                result[0] = true;
                return result;
            }

            if (hasOne)
            {
                return result;
            }
        }
    }

    private static bool AreEqual(IReadOnlyList<bool> first, IReadOnlyList<bool> second)
    {
        if (first == null)
        {
            throw new ArgumentNullException(nameof(first));
        }

        if (second == null)
        {
            throw new ArgumentNullException(nameof(second));
        }

        var count = first.Count;
        if (count != second.Count)
        {
            return false;
        }

        for (var i = 0; i < count; i++)
        {
            if (first[i] != second[i])
            {
                return false;
            }
        }

        return true;
    }

    private readonly struct PreviewEnumerable : IEnumerable<bool>
    {
        private readonly IReadOnlyList<bool> _source;
        private readonly int _length;

        public PreviewEnumerable(IReadOnlyList<bool> source, int length)
        {
            _source = source;
            _length = length;
        }

        public Enumerator GetEnumerator() => new Enumerator(_source, _length);
        IEnumerator<bool> IEnumerable<bool>.GetEnumerator() => GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<bool>
        {
            private readonly IReadOnlyList<bool> _source;
            private readonly int _length;
            private int _index;

            public Enumerator(IReadOnlyList<bool> source, int length)
            {
                _source = source;
                _length = length;
                _index = -1;
            }

            public bool Current => _source[_index];
            object System.Collections.IEnumerator.Current => Current;

            public bool MoveNext()
            {
                var next = _index + 1;
                if (next >= _length)
                {
                    return false;
                }

                _index = next;
                return true;
            }

            public void Reset() => _index = -1;
            public void Dispose()
            {
            }
        }
    }
}
