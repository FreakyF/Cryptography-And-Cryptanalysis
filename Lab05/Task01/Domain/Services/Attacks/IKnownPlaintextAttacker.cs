using Task01.Domain.Models;

namespace Task01.Domain.Services.Attacks;

public interface IKnownPlaintextAttacker
{
    AttackResult? Attack(string knownPlaintext, IReadOnlyList<bool> ciphertextBits, int lfsrDegree);
}