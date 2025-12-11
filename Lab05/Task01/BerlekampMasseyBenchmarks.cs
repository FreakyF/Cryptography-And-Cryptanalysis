using BenchmarkDotNet.Attributes;

namespace Task01;

[MemoryDiagnoser]
public class BerlekampMasseyBenchmarks
{
    private IBerlekampMasseySolver _solver = null!;
    private IReadOnlyList<bool> _sequenceShort = null!;
    private IReadOnlyList<bool> _sequenceMedium = null!;

    [GlobalSetup]
    public void Setup()
    {
        _solver = new BerlekampMasseySolver();

        _sequenceShort = BitConversions.IntArrayToBits([
            0, 0, 1, 0, 1, 1, 1, 0, 0, 1,
            0, 1, 1, 1
        ]);

        _sequenceMedium = BitConversions.IntArrayToBits([
            1, 0, 0, 1, 0,
            0, 0, 1, 1, 1,
            1, 0, 1, 0, 1,
            1, 0, 0, 1, 0,
            0, 0, 1, 1, 1
        ]);
    }

    [Benchmark]
    public BerlekampMasseyResult SolveShortBenchmark()
    {
        return _solver.Solve(_sequenceShort);
    }

    [Benchmark]
    public BerlekampMasseyResult SolveMediumBenchmark()
    {
        return _solver.Solve(_sequenceMedium);
    }
}