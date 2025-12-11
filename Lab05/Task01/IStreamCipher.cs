namespace Task01;

public interface IStreamCipher
{
    IReadOnlyList<bool> Encrypt(string plaintext, ILfsr lfsr);
    string Decrypt(IReadOnlyList<bool> ciphertextBits, ILfsr lfsr);
}