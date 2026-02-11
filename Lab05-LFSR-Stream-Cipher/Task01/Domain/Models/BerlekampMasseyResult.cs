namespace Task01.Domain.Models;

/// <summary>
/// Represents the result of the Berlekamp-Massey algorithm.
/// </summary>
/// <param name="ConnectionPolynomial">The connection polynomial coefficients found by the algorithm.</param>
/// <param name="LinearComplexity">The linear complexity of the sequence.</param>
public sealed record BerlekampMasseyResult(
    IReadOnlyList<bool> ConnectionPolynomial,
    int LinearComplexity);
