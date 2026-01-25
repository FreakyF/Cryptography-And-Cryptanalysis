namespace Task01.Domain.Services.LinearComplexity;

/// <summary>
/// Defines the contract for solving systems of linear equations over a Galois Field (specifically GF(2)).
/// </summary>
public interface IGaloisFieldSolver
{
    /// <summary>
    /// Solves the system of linear equations Ax = b over GF(2).
    /// </summary>
    /// <param name="matrix">The square matrix A.</param>
    /// <param name="vector">The result vector b.</param>
    /// <returns>The solution vector x, or null if no solution exists.</returns>
    bool[]? Solve(bool[,] matrix, bool[] vector);
}
