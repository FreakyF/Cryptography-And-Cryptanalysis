namespace Task01;

public interface IGaloisFieldSolver
{
    bool[]? Solve(bool[,] matrix, bool[] vector);
}