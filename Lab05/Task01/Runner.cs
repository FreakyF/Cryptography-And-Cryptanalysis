namespace Task01;

public sealed class Runner(bool benchmarkMode) : IRunner
{
    public Runner() : this(false)
    {
    }

    public void RunAll()
    {
        RunLfsrVerification();
        RunBerlekampMasseyVerification();
        RunFullAttack();
    }

    private void Log(string message)
    {
        if (!benchmarkMode)
        {
            Console.WriteLine(message);
        }
    }

    private void RunLfsrVerification()
    {
        Log("LFSR verification");
        Log(string.Empty);

        VerifyLfsr(
            [1, 1, 0],
            [0, 0, 1],
            [0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 1, 1, 1]);

        VerifyLfsr(
            [1, 0, 1, 0, 0],
            [1, 0, 0, 1, 0],
            [
                1, 0, 0, 1, 0,
                1, 1, 0, 0, 1,
                1, 1, 1, 1, 0,
                0, 0, 1, 1, 0,
                1, 1, 1, 0, 1
            ]);

        VerifyLfsr(
            [1, 1, 0, 1, 0],
            [1, 0, 0, 1, 0],
            [
                1, 0, 0, 1, 0,
                0, 0, 1, 1, 1,
                1, 0, 1, 0, 1,
                1, 0, 0, 1, 0,
                0, 0, 1, 1, 1
            ]);
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
        Log("Initial state (from ILfsr): " + string.Join("", BitConversions.BitsToIntArray(lfsr.State)));

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
            BitConversions.IntArrayToBits([
                0, 0, 1, 0, 1, 1, 1, 0, 0, 1,
                0, 1, 1, 1
            ]),
            BitConversions.IntArrayToBits([
                1, 0, 0, 1, 0,
                1, 1, 0, 0, 1,
                1, 1, 1, 1, 0,
                0, 0, 1, 1, 0,
                1, 1, 1, 0, 1
            ]),
            BitConversions.IntArrayToBits([
                1, 0, 0, 1, 0,
                0, 0, 1, 1, 1,
                1, 0, 1, 0, 1,
                1, 0, 0, 1, 0,
                0, 0, 1, 1, 1
            ])
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
        var feedback = GenerateRandomNonZeroVector(random, m, true);
        var initialState = GenerateRandomNonZeroVector(random, m, false);

        Log("Secret LFSR degree m = " + m);
        Log("Secret feedback coefficients p (p0..p" + (m - 1) + "): " +
            string.Join("", BitConversions.BitsToIntArray(feedback)));
        Log("Secret initial state sigma0: " + string.Join("", BitConversions.BitsToIntArray(initialState)));
        Log(string.Empty);

        ILfsr lfsr = new Lfsr(feedback, initialState);
        IStreamCipher cipher = new StreamCipher();

        const string plaintext = "This is a secret message for the LFSR stream cipher laboratory.";
        var ciphertextBits = cipher.Encrypt(plaintext, lfsr);

        Log("Plaintext length (characters): " + plaintext.Length);
        Log("Ciphertext bit length: " + ciphertextBits.Count);

        var previewLength = Math.Min(128, ciphertextBits.Count);
        var previewBits = ciphertextBits.Take(previewLength);
        Log("Ciphertext bits (first " + previewLength + "): " +
            BitConversions.BitsToBitString(previewBits));
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
            Log("Attack failed.");
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
        Log("Attack success: " + success);
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

            for (var i = 0; i < length; i++)
            {
                result[i] = random.Next(2) == 1;
            }

            if (forceFirstBitOne)
            {
                result[0] = true;
            }

            if (result.Any(bit => bit))
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

        if (first.Count != second.Count)
        {
            return false;
        }

        return !first.Where((t, i) => t != second[i]).Any();
    }
}