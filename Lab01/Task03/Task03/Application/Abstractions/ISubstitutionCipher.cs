using Task03.Domain;

namespace Task03.Application.Abstractions;

public interface ISubstitutionCipher
{
    string Encrypt(string normalizedPlaintext, SubstitutionKey key);
    string Decrypt(string normalizedCiphertext, SubstitutionKey key);
}