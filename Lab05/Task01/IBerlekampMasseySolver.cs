namespace Task01;

public interface IBerlekampMasseySolver
{
    BerlekampMasseyResult Solve(IReadOnlyList<bool> sequence);
}