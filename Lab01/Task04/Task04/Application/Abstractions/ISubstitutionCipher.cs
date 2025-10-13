using Task04.Domain;

namespace Task04.Application.Abstractions;

public interface ISubstitutionCipher
{
    string Encrypt(string normalizedPlaintext, SubstitutionKey key);
    string Decrypt(string normalizedCiphertext, SubstitutionKey key);
}