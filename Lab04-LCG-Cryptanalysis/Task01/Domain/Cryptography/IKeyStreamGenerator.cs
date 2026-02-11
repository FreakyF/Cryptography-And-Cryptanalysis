using System.Numerics;

namespace Task01.Domain.Cryptography;

public interface IKeyStreamGenerator
{
    bool NextBit();
    void Reset(BigInteger seed);
}