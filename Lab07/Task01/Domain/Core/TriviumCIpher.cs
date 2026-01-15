namespace Task01.Domain.Core;

using Shared;

public class TriviumCipher : ITriviumCipher
{
    private readonly bool[] _state = new bool[288];

    public void Initialize(bool[] key, bool[] iv, int warmupRounds = 1152)
    {
        Array.Clear(_state, 0, _state.Length);

        for (var i = 0; i < 80; i++)
        {
            _state[i] = key[i];
            _state[i + 93] = iv[i];
        }

        _state[285] = true;
        _state[286] = true;
        _state[287] = true;

        for (var i = 0; i < warmupRounds; i++)
        {
            UpdateState();
        }
    }

    public bool GenerateBit()
    {
        var t1 = _state[65] ^ _state[92];
        var t2 = _state[161] ^ _state[176];
        var t3 = _state[242] ^ _state[287];

        var z = t1 ^ t2 ^ t3;

        UpdateState();
        return z;
    }

    private void UpdateState()
    {
        var t1 = _state[65] ^ _state[92];
        var t2 = _state[161] ^ _state[176];
        var t3 = _state[242] ^ _state[287];

        t1 ^= (_state[90] && _state[91]) ^ _state[170];
        t2 ^= (_state[174] && _state[175]) ^ _state[263];
        t3 ^= (_state[285] && _state[286]) ^ _state[68];

        Array.Copy(_state, 0, _state, 1, 92);
        _state[0] = t3;

        Array.Copy(_state, 93, _state, 94, 83);
        _state[93] = t1;

        Array.Copy(_state, 177, _state, 178, 110);
        _state[177] = t2;
    }

    public bool[] GenerateKeystream(int length)
    {
        var keystream = new bool[length];
        for (var i = 0; i < length; i++)
        {
            keystream[i] = GenerateBit();
        }
        return keystream;
    }

    public byte[] Encrypt(byte[] plaintext)
    {
        var keystreamBits = GenerateKeystream(plaintext.Length * 8);
        var keystreamBytes = keystreamBits.ToByteArray();
        
        var result = new byte[plaintext.Length];
        for (var i = 0; i < plaintext.Length; i++)
        {
            result[i] = (byte)(plaintext[i] ^ keystreamBytes[i]);
        }
        return result;
    }

    public byte[] Decrypt(byte[] ciphertext) => Encrypt(ciphertext);

    public (int OnesCount, double Balance) GetStateStatistics()
    {
        var ones = _state.Count(b => b);
        return (ones, ones / 288.0);
    }
}