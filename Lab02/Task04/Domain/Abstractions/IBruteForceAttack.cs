using Task04.Domain.Models;

namespace Task04.Domain.Abstractions;

public interface IBruteForceAttack
{
    BruteForceResult BreakCipher(string cipherText);
}