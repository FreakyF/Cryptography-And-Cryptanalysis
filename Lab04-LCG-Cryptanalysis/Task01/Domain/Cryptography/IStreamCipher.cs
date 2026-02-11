using System.Numerics;

namespace Task01.Domain.Cryptography;

public interface IStreamCipher
{
    bool[] Encrypt(string plaintext, IKeyStreamGenerator generator);
    string Decrypt(IReadOnlyList<bool> ciphertext, BigInteger seed, IKeyStreamGenerator generator);
}