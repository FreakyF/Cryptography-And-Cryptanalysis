namespace Task01.Domain.Math;

public static class Gf2Solver
{
    public static bool[] SolveLinearSystem(List<bool[]> matrix, bool[] results, int variableCount)
    {
        var rowCount = matrix.Count;
        var augmented = BuildAugmentedMatrix(matrix, results, rowCount, variableCount);

        PerformGaussianElimination(augmented, rowCount, variableCount);

        return ExtractSolution(augmented, rowCount, variableCount);
    }

    private static bool[,] BuildAugmentedMatrix(List<bool[]> matrix, bool[] results, int rowCount, int variableCount)
    {
        var augmented = new bool[rowCount, variableCount + 1];
        for (var r = 0; r < rowCount; r++)
        {
            for (var c = 0; c < variableCount; c++)
                augmented[r, c] = matrix[r][c];
            
            augmented[r, variableCount] = results[r];
        }
        return augmented;
    }

    private static void PerformGaussianElimination(bool[,] augmented, int rowCount, int variableCount)
    {
        var pivotRow = 0;
        for (var col = 0; col < variableCount && pivotRow < rowCount; col++)
        {
            var sel = FindPivotRow(augmented, pivotRow, rowCount, col);
            if (sel == -1) continue;

            SwapRows(augmented, pivotRow, sel, variableCount);
            EliminateColumn(augmented, pivotRow, rowCount, col, variableCount);
            
            pivotRow++;
        }
    }

    private static int FindPivotRow(bool[,] augmented, int startRow, int rowCount, int col)
    {
        for (var row = startRow; row < rowCount; row++)
        {
            if (augmented[row, col]) return row;
        }
        return -1;
    }

    private static void SwapRows(bool[,] matrix, int row1, int row2, int colCount)
    {
        if (row1 == row2) return;
        for (var k = 0; k <= colCount; k++)
        {
            (matrix[row1, k], matrix[row2, k]) = (matrix[row2, k], matrix[row1, k]);
        }
    }

    private static void EliminateColumn(bool[,] augmented, int pivotRow, int rowCount, int col, int variableCount)
    {
        for (var row = 0; row < rowCount; row++)
        {
            if (row == pivotRow || !augmented[row, col])
            {
                continue;
            }

            for (var k = col; k <= variableCount; k++)
                augmented[row, k] ^= augmented[pivotRow, k];
        }
    }

    private static bool[] ExtractSolution(bool[,] augmented, int rowCount, int variableCount)
    {
        var solution = new bool[variableCount];
        for (var col = 0; col < variableCount; col++)
        {
            var pivotR = FindPivotRow(augmented, 0, rowCount, col);
            if (pivotR != -1)
            {
                solution[col] = augmented[pivotR, variableCount];
            }
        }
        return solution;
    }
}