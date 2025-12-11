using BenchmarkDotNet.Attributes;

namespace Task01;

[MemoryDiagnoser]
[SimpleJob]
public class CryptoBenchmarks
{
    private const int LfsrDegree = 8;
    private IKnownPlaintextAttacker _attacker = null!;
    private IReadOnlyList<bool> _bmSequence = null!;
    private IBerlekampMasseySolver _bmSolver = null!;
    private IReadOnlyList<bool> _ciphertext = null!;
    private IGaloisFieldSolver _gaussianSolver = null!;
    private string _knownPlaintext = null!;
    private ILfsr _lfsrMedium = null!;
    private ILfsr _lfsrSmall = null!;

    [GlobalSetup]
    public void Setup()
    {
        _lfsrSmall = new Lfsr(
            BitConversions.IntArrayToBits([1, 1, 0]),
            BitConversions.IntArrayToBits([0, 0, 1])
        );

        _lfsrMedium = new Lfsr(
            BitConversions.IntArrayToBits([1, 0, 1, 0, 0]),
            BitConversions.IntArrayToBits([1, 0, 0, 1, 0])
        );

        _bmSequence = BitConversions.IntArrayToBits([
            1, 0, 0, 1, 0,
            0, 0, 1, 1, 1,
            1, 0, 1, 0, 1,
            1, 0, 0, 1, 0,
            0, 0, 1, 1, 1
        ]);

        _bmSolver = new BerlekampMasseySolver();

        _gaussianSolver = new GaussianEliminationSolver();
        _attacker = new KnownPlaintextAttacker(_gaussianSolver);

        var feedback = BitConversions.IntArrayToBits([1, 1, 0, 0, 0, 1, 0, 0]);
        var initialState = BitConversions.IntArrayToBits([0, 0, 0, 0, 1, 1, 1, 0]);

        const string plaintext = "This is a secret message for the LFSR stream cipher laboratory.";
        ILfsr secretLfsr = new Lfsr(feedback, initialState);
        IStreamCipher cipher = new StreamCipher();

        _ciphertext = cipher.Encrypt(plaintext, secretLfsr);

        const int minimalKnownBits = 2 * LfsrDegree;
        var minimalKnownCharacters = (int)Math.Ceiling(minimalKnownBits / 8.0);
        _knownPlaintext = plaintext[..minimalKnownCharacters];
    }

    [Benchmark]
    public IReadOnlyList<bool> LfsrSmallGenerate()
    {
        _lfsrSmall.Reset(BitConversions.IntArrayToBits([0, 0, 1]));
        return _lfsrSmall.GenerateBits(64);
    }

    [Benchmark]
    public IReadOnlyList<bool> LfsrMediumGenerate()
    {
        _lfsrMedium.Reset(BitConversions.IntArrayToBits([1, 0, 0, 1, 0]));
        return _lfsrMedium.GenerateBits(256);
    }

    [Benchmark]
    public BerlekampMasseyResult BerlekampMasseySolve()
    {
        return _bmSolver.Solve(_bmSequence);
    }

    [Benchmark]
    public AttackResult? KnownPlaintextAttack()
    {
        return _attacker.Attack(_knownPlaintext, _ciphertext, LfsrDegree);
    }
}