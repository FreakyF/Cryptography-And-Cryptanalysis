using Task02.Domain.Models;

namespace Task02.Domain.Abstractions;

public interface IBruteForceAttack
{
    BruteForceResult BreakCipher(string cipherText);
}