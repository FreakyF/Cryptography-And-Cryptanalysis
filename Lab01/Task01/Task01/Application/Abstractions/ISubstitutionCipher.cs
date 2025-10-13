using Task01.Domain;

namespace Task01.Application.Abstractions;

public interface ISubstitutionCipher
{
    string Encrypt(string normalizedPlaintext, SubstitutionKey key);
    string Decrypt(string normalizedCiphertext, SubstitutionKey key);
}