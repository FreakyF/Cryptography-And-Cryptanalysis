namespace Task01;

public sealed class Runner : IRunner
{
    public void RunAll()
    {
        RunLfsrVerification();
        RunBerlekampMasseyVerification();
        RunFullAttack();
    }

    private static void RunLfsrVerification()
    {
        Console.WriteLine("LFSR verification");
        Console.WriteLine();

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

    private static void VerifyLfsr(int[] feedbackInts, int[] stateInts, int[] expectedInts)
    {
        var feedback = BitConversions.IntArrayToBits(feedbackInts);
        var state = BitConversions.IntArrayToBits(stateInts);
        var expected = BitConversions.IntArrayToBits(expectedInts);

        ILfsr lfsr = new Lfsr(feedback, state);

        Console.WriteLine("LFSR degree: " + lfsr.Degree);
        Console.WriteLine("Feedback coefficients (from ILfsr): " +
                          string.Join("", BitConversions.BitsToIntArray(lfsr.FeedbackCoefficients)));
        Console.WriteLine("Initial state (from ILfsr): " + string.Join("", BitConversions.BitsToIntArray(lfsr.State)));

        lfsr.Reset(state);

        var output = lfsr.GenerateBits(expected.Count);

        Console.WriteLine("Feedback coefficients (expected): " + string.Join("", feedbackInts));
        Console.WriteLine("Initial state (expected): " + string.Join("", stateInts));
        Console.WriteLine("Expected sequence: " + string.Join("", expectedInts));
        Console.WriteLine("Generated sequence: " + string.Join("", BitConversions.BitsToIntArray(output)));
        Console.WriteLine();
    }

    private static void RunBerlekampMasseyVerification()
    {
        Console.WriteLine("Berlekamp–Massey verification");
        Console.WriteLine();

        var sequences = new[]
        {
            BitConversions.IntArrayToBits([
                0, 0, 1, 0, 1, 1, 1, 0, 0, 1,
                0, 1, 1, 1
            ]),
            BitConversions.IntArrayToBits([
                1, 0, 0, 1, 0,
                1, 1, 0, 0, 1,
                1, 1, 1, 1, 1,
                0, 0, 0, 1, 1,
                0, 1, 1, 1, 0,
                1
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

            Console.WriteLine("Sequence " + (i + 1));
            Console.WriteLine("Linear complexity: " + result.LinearComplexity);
            Console.WriteLine("Connection polynomial coefficients: " +
                              string.Join("", BitConversions.BitsToIntArray(result.ConnectionPolynomial)));
            Console.WriteLine();
        }
    }

    private static void RunFullAttack()
    {
        Console.WriteLine("Full known-plaintext attack demonstration");
        Console.WriteLine();

        const int m = 8;

        var random = new Random();
        var feedback = GenerateRandomNonZeroVector(random, m, true);
        var initialState = GenerateRandomNonZeroVector(random, m, false);

        Console.WriteLine("Secret LFSR degree m = " + m);
        Console.WriteLine("Secret feedback coefficients p (p0..p" + (m - 1) + "): " +
                          string.Join("", BitConversions.BitsToIntArray(feedback)));
        Console.WriteLine(
            "Secret initial state sigma0: " + string.Join("", BitConversions.BitsToIntArray(initialState)));
        Console.WriteLine();

        ILfsr lfsr = new Lfsr(feedback, initialState);
        IStreamCipher cipher = new StreamCipher();

        const string plaintext = "This is a secret message for the LFSR stream cipher laboratory.";
        var ciphertextBits = cipher.Encrypt(plaintext, lfsr);

        Console.WriteLine("Plaintext length (characters): " + plaintext.Length);
        Console.WriteLine("Ciphertext bit length: " + ciphertextBits.Count);

        var previewLength = Math.Min(128, ciphertextBits.Count);
        var previewBits = ciphertextBits.Take(previewLength);
        Console.WriteLine("Ciphertext bits (first " + previewLength + "): " +
                          BitConversions.BitsToBitString(previewBits));
        Console.WriteLine();

        IGaloisFieldSolver solver = new GaussianEliminationSolver();
        IKnownPlaintextAttacker attacker = new KnownPlaintextAttacker(solver);

        const int minimalKnownBits = 2 * m;
        var minimalKnownCharacters = (int)Math.Ceiling(minimalKnownBits / 8.0);

        var knownCharacters = Math.Min(minimalKnownCharacters, plaintext.Length);
        var knownPlaintext = plaintext[..knownCharacters];

        var testBits = BitConversions.BitStringToBits("01010101");
        var testBitString = BitConversions.BitsToBitString(testBits);
        Console.WriteLine("BitStringToBits/BitsToBitString test: " + testBitString);

        var knownBits = BitConversions.StringToBits(knownPlaintext);

        Console.WriteLine("Known plaintext used for attack: " + knownPlaintext);
        Console.WriteLine("Known plaintext bits: " + BitConversions.BitsToBitString(knownBits));
        Console.WriteLine("Known bits count: " + knownBits.Count);
        Console.WriteLine();

        var attackResult = attacker.Attack(knownPlaintext, ciphertextBits, m);

        if (attackResult == null)
        {
            Console.WriteLine("Attack failed.");
            return;
        }

        Console.WriteLine("Recovered keystream bits (from known segment): " +
                          BitConversions.BitsToBitString(attackResult.KeyStream));
        Console.WriteLine("Recovered feedback coefficients: " +
                          string.Join("", BitConversions.BitsToIntArray(attackResult.FeedbackCoefficients)));
        Console.WriteLine("Recovered initial state: " +
                          string.Join("", BitConversions.BitsToIntArray(attackResult.InitialState)));
        Console.WriteLine();

        var feedbackMatch = AreEqual(feedback, attackResult.FeedbackCoefficients);
        var initialStateMatch = AreEqual(initialState, attackResult.InitialState);

        Console.WriteLine("Feedback coefficients match: " + feedbackMatch);
        Console.WriteLine("Initial state matches: " + initialStateMatch);
        Console.WriteLine();

        ILfsr recoveredLfsr = new Lfsr(attackResult.FeedbackCoefficients, attackResult.InitialState);
        IStreamCipher recoveredCipher = new StreamCipher();
        var recoveredPlaintext = recoveredCipher.Decrypt(ciphertextBits, recoveredLfsr);

        Console.WriteLine("Recovered plaintext:");
        Console.WriteLine(recoveredPlaintext);
        Console.WriteLine();

        var success = string.Equals(plaintext, recoveredPlaintext, StringComparison.Ordinal);
        Console.WriteLine("Attack success: " + success);
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