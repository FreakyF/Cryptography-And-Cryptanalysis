namespace Task01.Domain.Core;

public interface ITriviumCipher
{
    void Initialize(byte[] key, byte[] iv, int warmupRounds = 1152);
    bool GenerateBit();
    byte[] GenerateKeystream(int length);
    byte[] Encrypt(byte[] plaintext);
    byte[] Decrypt(byte[] ciphertext);
    (int OnesCount, double Balance) GetStateStatistics();
}