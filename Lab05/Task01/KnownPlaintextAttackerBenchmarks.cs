using BenchmarkDotNet.Attributes;

namespace Task01;

[MemoryDiagnoser]
public class KnownPlaintextAttackerBenchmarks
{
    private IGaloisFieldSolver _solver = null!;
    private IKnownPlaintextAttacker _attacker = null!;
    private IReadOnlyList<bool> _ciphertext = null!;
    private string _knownPlaintext = null!;
    private const int Degree = 8;

    [GlobalSetup]
    public void Setup()
    {
        _solver = new GaussianEliminationSolver();
        _attacker = new KnownPlaintextAttacker(_solver);

        var feedback = BitConversions.IntArrayToBits(new[] { 1, 1, 0, 0, 0, 1, 0, 0 });
        var initialState = BitConversions.IntArrayToBits(new[] { 0, 0, 0, 0, 1, 1, 1, 0 });

        const string plaintext = "This is a secret message for the LFSR stream cipher laboratory.";
        ILfsr lfsr = new Lfsr(feedback, initialState);
        IStreamCipher cipher = new StreamCipher();

        _ciphertext = cipher.Encrypt(plaintext, lfsr);

        const int minimalKnownBits = 2 * Degree;
        var minimalKnownCharacters = (int)Math.Ceiling(minimalKnownBits / 8.0);
        _knownPlaintext = plaintext[..minimalKnownCharacters];
    }

    [Benchmark]
    public AttackResult? KnownPlaintextAttackBenchmark()
    {
        return _attacker.Attack(_knownPlaintext, _ciphertext, Degree);
    }
}