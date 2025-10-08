using Task01.Domain;

namespace Task01.Infrastructure;

public sealed class SubstitutionCipher : ICipher
{
    private readonly Dictionary<char, char> _encryptMap;
    private readonly Dictionary<char, char> _decryptMap;

    public SubstitutionCipher(Dictionary<char, char> keyMap)
    {
        if (keyMap.Count != 26)
        {
            throw new ArgumentException($"Invalid key map: expected 26 unique mappings, but got ({keyMap.Count}). " +
                                        $"The key file must define a substitution for every letter A–Z.");
        }

        _encryptMap = keyMap;
        _decryptMap = keyMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public string Encrypt(string text) => new(text.Select(c => _encryptMap.GetValueOrDefault(c, c)).ToArray());

    public string Decrypt(string text) => new(text.Select(c => _decryptMap.GetValueOrDefault(c, c)).ToArray());
}