using System.Numerics;

namespace Task01.Domain.Attack;

public interface IKnownPlaintextAttacker
{
    AttackResult RecoverParameters(string knownPlaintext, IReadOnlyList<bool> ciphertext, BigInteger modulus,
        int stateBitLength);
}