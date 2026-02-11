using Task01.Domain.Models;

namespace Task01.Domain.Services.LinearComplexity;

/// <summary>
/// Defines the contract for an algorithm that calculates the linear complexity and connection polynomial of a binary sequence.
/// </summary>
public interface IBerlekampMasseySolver
{
    /// <summary>
    /// Computes the minimal connection polynomial and linear complexity of the given binary sequence.
    /// </summary>
    /// <param name="sequence">The binary sequence to analyze.</param>
    /// <returns>A <see cref="BerlekampMasseyResult"/> containing the polynomial and complexity.</returns>
    BerlekampMasseyResult Solve(IReadOnlyList<bool> sequence);
}
