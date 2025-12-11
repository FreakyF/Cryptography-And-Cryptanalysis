namespace Task01.Domain.Services.LinearComplexity;

public interface IGaloisFieldSolver
{
    bool[]? Solve(bool[,] matrix, bool[] vector);
}