using System.Text;

namespace Task01;

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
