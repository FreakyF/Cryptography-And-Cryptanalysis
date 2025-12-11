using System.Runtime.CompilerServices;
using Task01.Domain.Models;
using Task01.Domain.Services.LinearComplexity;
using Task01.Domain.Utils;

namespace Task01.Domain.Services.Attacks;

public sealed class KnownPlaintextAttacker(IGaloisFieldSolver solver) : IKnownPlaintextAttacker
{
    private readonly IGaloisFieldSolver _solver = solver ?? throw new ArgumentNullException(nameof(solver));

    private int _degree;
    private int _requiredBits;
    private bool[,]? _matrix;
    private bool[]? _vector;
    private bool[]? _knownBits;
    private bool[]? _keyStream;
    private bool[]? _initialState;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public AttackResult? Attack(string knownPlaintext, IReadOnlyList<bool> ciphertextBits, int lfsrDegree)
    {
        if (knownPlaintext == null)
        {
            throw new ArgumentNullException(nameof(knownPlaintext));
        }

        if (ciphertextBits == null)
        {
            throw new ArgumentNullException(nameof(ciphertextBits));
        }

        if (lfsrDegree <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lfsrDegree));
        }

        EnsureBuffers(lfsrDegree);

        if (ciphertextBits.Count < _requiredBits)
        {
            throw new ArgumentException("Ciphertext does not contain enough bits for the attack.",
                nameof(ciphertextBits));
        }

        var knownBitsUtf8 = BitConversions.StringToBits(knownPlaintext);
        if (knownBitsUtf8.Count < _requiredBits)
        {
            return null;
        }

        var knownBits = _knownBits!;
        if (knownBitsUtf8 is bool[] knownArray && knownArray.Length >= _requiredBits)
        {
            Array.Copy(knownArray, knownBits, _requiredBits);
        }
        else
        {
            for (var i = 0; i < _requiredBits; i++)
            {
                knownBits[i] = knownBitsUtf8[i];
            }
        }

        var keyStream = _keyStream!;
        for (var i = 0; i < _requiredBits; i++)
        {
            keyStream[i] = knownBits[i] ^ ciphertextBits[i];
        }

        var matrix = _matrix!;
        var vector = _vector!;
        var degree = _degree;

        for (var row = 0; row < degree; row++)
        {
            var offset = row;

            for (var col = 0; col < degree; col++)
            {
                matrix[row, col] = keyStream[offset + col];
            }

            vector[row] = keyStream[row + degree];
        }

        var feedback = _solver.Solve(matrix, vector);
        if (feedback == null)
        {
            return null;
        }

        var initialState = _initialState!;
        for (var i = 0; i < degree; i++)
        {
            initialState[i] = keyStream[i];
        }

        return new AttackResult(feedback, initialState, keyStream);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void EnsureBuffers(int lfsrDegree)
    {
        if (_degree == lfsrDegree && _matrix != null)
        {
            return;
        }

        if (_degree != 0 && _degree != lfsrDegree)
        {
            throw new ArgumentException("KnownPlaintextAttacker instance supports only a single fixed degree.",
                nameof(lfsrDegree));
        }

        _degree = lfsrDegree;
        _requiredBits = lfsrDegree * 2;

        _matrix = new bool[lfsrDegree, lfsrDegree];
        _vector = GC.AllocateUninitializedArray<bool>(lfsrDegree);
        _knownBits = GC.AllocateUninitializedArray<bool>(_requiredBits);
        _keyStream = GC.AllocateUninitializedArray<bool>(_requiredBits);
        _initialState = GC.AllocateUninitializedArray<bool>(lfsrDegree);
    }
}
