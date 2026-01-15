namespace Task01.Domain.Core;

public interface ITriviumCipher
{   
    void Initialize(bool[] key, bool[] iv, int warmupRounds = 1152);
    bool GenerateBit();
    bool[] GenerateKeystream(int length);
    byte[] Encrypt(byte[] plaintext);
    byte[] Decrypt(byte[] ciphertext);
    (int OnesCount, double Balance) GetStateStatistics();
}