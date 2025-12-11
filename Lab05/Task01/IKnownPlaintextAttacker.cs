namespace Task01;

public interface IKnownPlaintextAttacker
{
    AttackResult? Attack(string knownPlaintext, IReadOnlyList<bool> ciphertextBits, int lfsrDegree);
}