namespace Task01;

public sealed class KnownPlaintextAttacker(IGaloisFieldSolver solver) : IKnownPlaintextAttacker
{
    private readonly IGaloisFieldSolver _solver = solver ?? throw new ArgumentNullException(nameof(solver));

    public AttackResult? Attack(string knownPlaintext, IReadOnlyList<bool> ciphertextBits, int lfsrDegree)
    {
        if (knownPlaintext == null)
        {
            throw new ArgumentNullException(nameof(knownPlaintext));
        }

        if (ciphertextBits == null)
        {
            throw new ArgumentNullException(nameof(ciphertextBits));
        }

        if (lfsrDegree <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lfsrDegree));
        }

        var knownBits = BitConversions.StringToBits(knownPlaintext);

        if (ciphertextBits.Count < knownBits.Count)
        {
            throw new ArgumentException("Ciphertext is shorter than known plaintext in bits.", nameof(ciphertextBits));
        }

        var keyStream = new List<bool>(knownBits.Count);
        keyStream.AddRange(knownBits.Select((t, i) => t ^ ciphertextBits[i]));

        if (keyStream.Count < 2 * lfsrDegree)
        {
            return null;
        }

        var matrix = new bool[lfsrDegree, lfsrDegree];
        var vector = new bool[lfsrDegree];

        for (var i = 0; i < lfsrDegree; i++)
        {
            for (var j = 0; j < lfsrDegree; j++)
            {
                matrix[i, j] = keyStream[i + j];
            }

            vector[i] = keyStream[i + lfsrDegree];
        }

        var feedback = _solver.Solve(matrix, vector);
        if (feedback == null)
        {
            return null;
        }

        var initialState = keyStream.Take(lfsrDegree).ToArray();

        return new AttackResult(feedback, initialState, keyStream.ToArray());
    }
}