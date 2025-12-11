using BenchmarkDotNet.Attributes;

namespace Task01;

[MemoryDiagnoser]
public class LfsrBenchmarks
{
    private ILfsr _lfsrSmall = null!;
    private ILfsr _lfsrMedium = null!;
    private IReadOnlyList<bool> _stateSmall = null!;
    private IReadOnlyList<bool> _stateMedium = null!;

    [GlobalSetup]
    public void Setup()
    {
        _stateSmall = BitConversions.IntArrayToBits(new[] { 0, 0, 1 });
        _lfsrSmall = new Lfsr(
            BitConversions.IntArrayToBits(new[] { 1, 1, 0 }),
            _stateSmall
        );

        _stateMedium = BitConversions.IntArrayToBits(new[] { 1, 0, 0, 1, 0 });
        _lfsrMedium = new Lfsr(
            BitConversions.IntArrayToBits(new[] { 1, 0, 1, 0, 0 }),
            _stateMedium
        );
    }

    [Benchmark]
    public bool NextBitSmallBenchmark()
    {
        _lfsrSmall.Reset(_stateSmall);
        return _lfsrSmall.NextBit();
    }

    [Benchmark]
    public IReadOnlyList<bool> GenerateBitsSmallBenchmark()
    {
        _lfsrSmall.Reset(_stateSmall);
        return _lfsrSmall.GenerateBits(64);
    }

    [Benchmark]
    public IReadOnlyList<bool> GenerateBitsMediumBenchmark()
    {
        _lfsrMedium.Reset(_stateMedium);
        return _lfsrMedium.GenerateBits(256);
    }

    [Benchmark]
    public void ResetBenchmark()
    {
        _lfsrMedium.Reset(_stateMedium);
    }
}