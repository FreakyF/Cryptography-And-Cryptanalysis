using System.Numerics;

namespace Task01.Domain.Math;

/// <summary>
///     Provides mathematical utilities for solving systems of linear equations over the Galois Field GF(2).
/// </summary>
/// <remarks>
///     This class implements Gaussian elimination optimized for boolean systems using bit-packed operations.
///     It is specifically designed to handle systems where coefficients and unknowns are binary values (0 or 1),
///     and arithmetic operations are performed modulo 2 (equivalent to bitwise XOR).
/// </remarks>
public static class Gf2Solver
{
    /// <summary>
    ///     Solves a system of linear equations over GF(2) using Gaussian elimination.
    /// </summary>
    /// <param name="matrix">
    ///     The coefficient matrix represented as a list of boolean arrays. Each array corresponds to a row in the system.
    ///     The length of each boolean array must match <paramref name="variableCount"/>.
    /// </param>
    /// <param name="results">
    ///     The results vector (right-hand side of the equations). The length must match the number of rows in <paramref name="matrix"/>.
    /// </param>
    /// <param name="variableCount">
    ///     The number of variables (unknowns) in the system.
    ///     <note type="warning">
    ///         This implementation uses a fixed 128-bit structure (<see cref="Row"/>) to store row data plus the result bit.
    ///         Therefore, it supports a maximum of 127 variables.
    ///     </note>
    /// </param>
    /// <returns>
    ///     A boolean array representing the solution vector. If the system is inconsistent or underdetermined,
    ///     the method attempts to return a valid partial assignment, but no explicit exception is thrown for singular matrices
    ///     in this specific implementation path.
    /// </returns>
    public static bool[] SolveLinearSystem(List<bool[]> matrix, bool[] results, int variableCount)
    {
        var rowCount = matrix.Count;
        var rows = new Row[rowCount];

        // Initialize the optimized row representations from the input boolean matrix
        for (var i = 0; i < rowCount; i++)
        {
            for (var j = 0; j < variableCount; j++)
            {
                if (matrix[i][j])
                {
                    if (j < 64)
                    {
                        rows[i].Low |= 1UL << j;
                    }
                    else
                    {
                        rows[i].High |= 1UL << (j - 64);
                    }
                }
            }

            // Store the result bit in the highest bit of the High segment (bit 63)
            if (results[i])
            {
                rows[i].High |= 1UL << 63;
            }
        }

        // Perform Gaussian elimination (Forward Elimination)
        var pivotRow = 0;
        for (var col = 0; col < variableCount && pivotRow < rowCount; col++)
        {
            var sel = -1;
            // Find a pivot row for the current column
            for (var r = pivotRow; r < rowCount; r++)
            {
                var isSet = col < 64
                    ? (rows[r].Low & (1UL << col)) != 0
                    : (rows[r].High & (1UL << (col - 64))) != 0;
                if (isSet)
                {
                    sel = r;
                    break;
                }
            }

            if (sel == -1)
            {
                continue;
            }

            // Swap the current row with the pivot row
            (rows[pivotRow], rows[sel]) = (rows[sel], rows[pivotRow]);

            // Eliminate other rows
            for (var r = 0; r < rowCount; r++)
            {
                if (r != pivotRow)
                {
                    var isSet = col < 64
                        ? (rows[r].Low & (1UL << col)) != 0
                        : (rows[r].High & (1UL << (col - 64))) != 0;
                    if (isSet)
                    {
                        // Row operation: R_r = R_r XOR R_pivot (Arithmetic mod 2)
                        rows[r].Low ^= rows[pivotRow].Low;
                        rows[r].High ^= rows[pivotRow].High;
                    }
                }
            }

            pivotRow++;
        }

        // Back-substitution / Extract solution
        var solution = new bool[variableCount];
        for (var i = 0; i < rowCount; i++)
        {
            var firstBit = -1;
            if (rows[i].Low != 0)
            {
                firstBit = BitOperations.TrailingZeroCount(rows[i].Low);
            }
            else if ((rows[i].High & ~(1UL << 63)) != 0)
            {
                firstBit = 64 + BitOperations.TrailingZeroCount(rows[i].High & ~(1UL << 63));
            }

            if (firstBit != -1 && firstBit < variableCount)
            {
                // The result is stored in the 63rd bit of High
                solution[firstBit] = (rows[i].High & (1UL << 63)) != 0;
            }
        }

        return solution;
    }

    /// <summary>
    ///     Represents a row in the augmented matrix using a bit-packed format for performance.
    /// </summary>
    /// <remarks>
    ///     The row consists of two 64-bit integers (<see cref="ulong"/>), providing a total capacity of 128 bits.
    ///     Since the last bit of <see cref="High"/> (index 63) is reserved for the equation result (RHS),
    ///     this structure effectively supports systems with up to 127 variables.
    /// </remarks>
    private struct Row
    {
        /// <summary>
        ///     Stores the coefficients for variables 0 through 63.
        /// </summary>
        public ulong Low;

        /// <summary>
        ///     Stores the coefficients for variables 64 through 126.
        ///     Bit 63 is reserved for the augmented column (result) value.
        /// </summary>
        public ulong High;
    }
}
