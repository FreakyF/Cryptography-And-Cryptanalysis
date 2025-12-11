namespace Task01.Domain.Models;

public sealed record BerlekampMasseyResult(
    IReadOnlyList<bool> ConnectionPolynomial,
    int LinearComplexity);