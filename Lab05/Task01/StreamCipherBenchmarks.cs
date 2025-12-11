using BenchmarkDotNet.Attributes;

namespace Task01;

[MemoryDiagnoser]
public class StreamCipherBenchmarks
{
    private IStreamCipher _cipher = null!;
    private ILfsr _lfsr = null!;
    private IReadOnlyList<bool> _feedback = null!;
    private IReadOnlyList<bool> _initialState = null!;
    private string _plaintext = null!;
    private IReadOnlyList<bool> _ciphertext = null!;

    [GlobalSetup]
    public void Setup()
    {
        _cipher = new StreamCipher();
        _feedback = BitConversions.IntArrayToBits(new[] { 1, 1, 0, 0, 0, 1, 0, 0 });
        _initialState = BitConversions.IntArrayToBits(new[] { 0, 0, 0, 0, 1, 1, 1, 0 });
        _plaintext = "This is a secret message for the LFSR stream cipher laboratory.";

        _lfsr = new Lfsr(_feedback, _initialState);
        _ciphertext = _cipher.Encrypt(_plaintext, _lfsr);
    }

    [Benchmark]
    public IReadOnlyList<bool> EncryptBenchmark()
    {
        _lfsr = new Lfsr(_feedback, _initialState);
        return _cipher.Encrypt(_plaintext, _lfsr);
    }

    [Benchmark]
    public string DecryptBenchmark()
    {
        _lfsr = new Lfsr(_feedback, _initialState);
        return _cipher.Decrypt(_ciphertext, _lfsr);
    }
}