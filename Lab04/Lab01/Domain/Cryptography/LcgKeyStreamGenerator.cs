using System.Numerics;
using Lab01.Domain.Numeric;

namespace Lab01.Domain.Cryptography;

public sealed class LcgKeyStreamGenerator : IKeyStreamGenerator
{
    private readonly BigInteger _a;
    private readonly BigInteger _b;
    private readonly BigInteger _modulus;
    private readonly int _stateBitLength;
    private int _bitPosition;

    private BigInteger _state;
    private bool[] _stateBits = [];

    public LcgKeyStreamGenerator(BigInteger a, BigInteger b, BigInteger modulus, BigInteger seed, int stateBitLength)
    {
        _a = a;
        _b = b;
        _modulus = modulus;
        _stateBitLength = stateBitLength;
        Reset(seed);
    }

    public bool NextBit()
    {
        if (_bitPosition >= _stateBitLength || _stateBits.Length == 0)
        {
            _state = BigInteger.Remainder(_a * _state + _b, _modulus);
            _stateBits = BitConversion.ToFixedSizeBits(_state, _stateBitLength);
            _bitPosition = 0;
        }

        var bit = _stateBits[_bitPosition];
        _bitPosition++;
        return bit;
    }

    public void Reset(BigInteger seed)
    {
        _state = BigInteger.Remainder(seed, _modulus);

        if (_state.Sign < 0)
        {
            _state += _modulus;
        }

        _stateBits = [];
        _bitPosition = _stateBitLength;
    }
}