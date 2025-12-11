using Task01.Domain.Services.Lfsr;

namespace Task01.Domain.Services.StreamCipher;

public interface IStreamCipher
{
    IReadOnlyList<bool> Encrypt(string plaintext, ILfsr lfsr);
    string Decrypt(IReadOnlyList<bool> ciphertextBits, ILfsr lfsr);
}