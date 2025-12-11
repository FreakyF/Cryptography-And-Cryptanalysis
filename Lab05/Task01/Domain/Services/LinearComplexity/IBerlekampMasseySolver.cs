using Task01.Domain.Models;

namespace Task01.Domain.Services.LinearComplexity;

public interface IBerlekampMasseySolver
{
    BerlekampMasseyResult Solve(IReadOnlyList<bool> sequence);
}