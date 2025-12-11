namespace Task01;

public sealed record BerlekampMasseyResult(
    IReadOnlyList<bool> ConnectionPolynomial,
    int LinearComplexity);