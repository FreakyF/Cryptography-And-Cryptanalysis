using System.Numerics;

namespace Task01.Domain.Math;

public static class Gf2Solver
{
    public static bool[] SolveLinearSystem(List<bool[]> matrix, bool[] results, int variableCount)
    {
        var rowCount = matrix.Count;
        var rows = new Row[rowCount];

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

            if (results[i])
            {
                rows[i].High |= 1UL << 63;
            }
        }

        var pivotRow = 0;
        for (var col = 0; col < variableCount && pivotRow < rowCount; col++)
        {
            var sel = -1;
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

            (rows[pivotRow], rows[sel]) = (rows[sel], rows[pivotRow]);

            for (var r = 0; r < rowCount; r++)
            {
                if (r != pivotRow)
                {
                    var isSet = col < 64
                        ? (rows[r].Low & (1UL << col)) != 0
                        : (rows[r].High & (1UL << (col - 64))) != 0;
                    if (isSet)
                    {
                        rows[r].Low ^= rows[pivotRow].Low;
                        rows[r].High ^= rows[pivotRow].High;
                    }
                }
            }

            pivotRow++;
        }

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
                solution[firstBit] = (rows[i].High & (1UL << 63)) != 0;
            }
        }

        return solution;
    }

    private struct Row
    {
        public ulong Low;
        public ulong High;
    }
}