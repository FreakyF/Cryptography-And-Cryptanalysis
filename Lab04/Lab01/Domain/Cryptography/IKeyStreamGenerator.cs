using System.Numerics;

namespace Lab01.Domain.Cryptography;

public interface IKeyStreamGenerator
{
    bool NextBit();
    void Reset(BigInteger seed);
}