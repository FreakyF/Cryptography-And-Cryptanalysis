using System.Runtime.CompilerServices;

namespace Task01.Domain.Services.LinearComplexity;

/// <summary>
/// Provides a high-performance implementation of Gaussian elimination over GF(2)
/// to solve systems of linear equations.
/// </summary>
public sealed class GaussianEliminationSolver : IGaloisFieldSolver
{
    /// <summary>
    /// Solves the system of linear equations Ax = b over GF(2).
    /// </summary>
    /// <param name="matrix">The square matrix A (m x m).</param>
    /// <param name="vector">The result vector b (length m).</param>
    /// <returns>The solution vector x if a unique solution exists; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matrix"/> or <paramref name="vector"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the matrix is not square or dimensions do not match the vector.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool[]? Solve(bool[,] matrix, bool[] vector)
    {
        if (matrix == null)
        {
            throw new ArgumentNullException(nameof(matrix));
        }

        if (vector == null)
        {
            throw new ArgumentNullException(nameof(vector));
        }

        var m = vector.Length;

        if (matrix.GetLength(0) != m || matrix.GetLength(1) != m)
        {
            throw new ArgumentException("Matrix must be square and match vector length.", nameof(matrix));
        }

        if (m == 0)
        {
            return Array.Empty<bool>();
        }

        if (m > 63)
        {
            return SolveSlow(matrix, vector);
        }

        var rhsMask = 1UL << m;
        var leftMask = rhsMask - 1UL;

        Span<ulong> rows = stackalloc ulong[m];

        for (var row = 0; row < m; row++)
        {
            ulong rowMask = 0;

            for (var col = 0; col < m; col++)
            {
                if (matrix[row, col])
                {
                    rowMask |= 1UL << col;
                }
            }

            if (vector[row])
            {
                rowMask |= rhsMask;
            }

            rows[row] = rowMask;
        }

        for (var col = 0; col < m; col++)
        {
            var pivotBit = 1UL << col;
            var pivotRow = col;

            while (pivotRow < m && (rows[pivotRow] & pivotBit) == 0)
            {
                pivotRow++;
            }

            if (pivotRow == m)
            {
                continue;
            }

            if (pivotRow != col)
            {
                var tmp = rows[col];
                rows[col] = rows[pivotRow];
                rows[pivotRow] = tmp;
            }

            var pivotRowValue = rows[col];

            for (var row = col + 1; row < m; row++)
            {
                if ((rows[row] & pivotBit) != 0)
                {
                    rows[row] ^= pivotRowValue;
                }
            }
        }

        for (var row = 0; row < m; row++)
        {
            var r = rows[row];
            if ((r & leftMask) == 0 && (r & rhsMask) != 0)
            {
                return null;
            }
        }

        var solution = GC.AllocateUninitializedArray<bool>(m);

        for (var i = m - 1; i >= 0; i--)
        {
            var r = rows[i];
            var coeffs = r & leftMask;

            if (((coeffs >> i) & 1UL) == 0)
            {
                solution[i] = false;
                continue;
            }

            var value = (r & rhsMask) != 0;

            for (var j = i + 1; j < m; j++)
            {
                if (((coeffs >> j) & 1UL) != 0 && solution[j])
                {
                    value ^= true;
                }
            }

            solution[i] = value;
        }

        return solution;
    }

    /// <summary>
    /// Fallback solver for larger matrices (m > 63) using standard array operations.
    /// </summary>
    /// <param name="matrix">The matrix A.</param>
    /// <param name="vector">The vector b.</param>
    /// <returns>The solution vector x, or <c>null</c> if no solution exists.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool[]? SolveSlow(bool[,] matrix, bool[] vector)
    {
        var m = vector.Length;
        var augmented = new bool[m, m + 1];

        for (var row = 0; row < m; row++)
        {
            for (var col = 0; col < m; col++)
            {
                augmented[row, col] = matrix[row, col];
            }

            augmented[row, m] = vector[row];
        }

        for (var col = 0; col < m; col++)
        {
            var pivot = -1;
            for (var row = col; row < m; row++)
            {
                if (!augmented[row, col])
                {
                    continue;
                }

                pivot = row;
                break;
            }

            if (pivot == -1)
            {
                continue;
            }

            if (pivot != col)
            {
                for (var k = col; k <= m; k++)
                {
                    (augmented[col, k], augmented[pivot, k]) = (augmented[pivot, k], augmented[col, k]);
                }
            }

            for (var row = 0; row < m; row++)
            {
                if (row == col || !augmented[row, col])
                {
                    continue;
                }

                for (var k = col; k <= m; k++)
                {
                    augmented[row, k] ^= augmented[col, k];
                }
            }
        }

        for (var row = 0; row < m; row++)
        {
            var allZero = true;
            for (var col = 0; col < m; col++)
            {
                if (!augmented[row, col])
                {
                    continue;
                }

                allZero = false;
                break;
            }

            if (allZero && augmented[row, m])
            {
                return null;
            }
        }

        var solution = new bool[m];

        for (var i = m - 1; i >= 0; i--)
        {
            var value = augmented[i, m];

            for (var j = i + 1; j < m; j++)
            {
                if (augmented[i, j] && solution[j])
                {
                    value ^= true;
                }
            }

            solution[i] = value;
        }

        return solution;
    }
}
