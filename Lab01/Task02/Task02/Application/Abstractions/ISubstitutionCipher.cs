using Task02.Domain;

namespace Task02.Application.Abstractions;

public interface ISubstitutionCipher
{
    string Encrypt(string normalizedPlaintext, SubstitutionKey key);
    string Decrypt(string normalizedCiphertext, SubstitutionKey key);
}