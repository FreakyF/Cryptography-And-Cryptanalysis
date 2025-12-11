using BenchmarkDotNet.Attributes;

namespace Task01;

[MemoryDiagnoser]
public class GaussianEliminationBenchmarks
{
    private IGaloisFieldSolver _solver = null!;
    private bool[,] _matrix = null!;
    private bool[] _vector = null!;

    [GlobalSetup]
    public void Setup()
    {
        _solver = new GaussianEliminationSolver();

        var m = 16;
        _matrix = new bool[m, m];
        _vector = new bool[m];

        for (var i = 0; i < m; i++)
        {
            for (var j = 0; j < m; j++)
            {
                _matrix[i, j] = (i + j) % 3 == 0;
            }

            _vector[i] = i % 2 == 0;
        }
    }

    [Benchmark]
    public bool[]? SolveBenchmark()
    {
        return _solver.Solve(_matrix, _vector);
    }
}