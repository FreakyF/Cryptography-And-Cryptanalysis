using System.Runtime.CompilerServices;

namespace Task01;

public sealed class GaussianEliminationSolver : IGaloisFieldSolver
{
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