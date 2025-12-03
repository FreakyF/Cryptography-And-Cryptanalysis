using System.Numerics;
using Lab01.Domain.Numeric;

namespace Lab01.Domain.Attack;

public sealed class LcgKnownPlaintextAttacker : IKnownPlaintextAttacker
{
    public AttackResult RecoverParameters(
        string knownPlaintext,
        IReadOnlyList<bool> ciphertext,
        BigInteger modulus,
        int stateBitLength)
    {
        var keyStream = KeyStreamRecovery.RecoverFromKnownPlaintext(knownPlaintext, ciphertext);
        var requiredBits = 3 * stateBitLength;

        if (keyStream.Length < requiredBits)
        {
            return AttackResult.InsufficientKeystream(requiredBits, keyStream.Length);
        }

        var s1Bits = BitConversion.Slice(keyStream, 0, stateBitLength);
        var s2Bits = BitConversion.Slice(keyStream, stateBitLength, stateBitLength);
        var s3Bits = BitConversion.Slice(keyStream, 2 * stateBitLength, stateBitLength);

        var s1 = BitConversion.BitsToBigInteger(s1Bits);
        var s2 = BitConversion.BitsToBigInteger(s2Bits);
        var s3 = BitConversion.BitsToBigInteger(s3Bits);

        var lambda = ModularArithmetic.NormalizeMod(s2 - s3, modulus);
        var mu = ModularArithmetic.NormalizeMod(s1 - s2, modulus);

        var (delta, _) = ModularArithmetic.ExtendedGcd(mu, modulus);

        if (delta != BigInteger.One)
        {
            return AttackResult.AmbiguousSolutions(delta);
        }

        var muInverse = ModularArithmetic.ModInverse(mu, modulus);

        if (muInverse == null)
        {
            return AttackResult.Failed("Modular inverse does not exist despite gcd(mu, m) = 1.");
        }

        var a = ModularArithmetic.NormalizeMod(lambda * muInverse.Value, modulus);
        var b = ModularArithmetic.NormalizeMod(s2 - s1 * a, modulus);
        var s3Check = ModularArithmetic.NormalizeMod(a * s2 + b, modulus);

        return s3Check != s3 ? AttackResult.VerificationFailed() : AttackResult.Succeeded(a, b, delta);
    }
}