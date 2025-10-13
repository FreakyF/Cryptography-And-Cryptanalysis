using System.Text;
using Task02.Application.Abstractions;
using Task02.Domain;

namespace Task02.Application.Cipher;

public sealed class SubstitutionCipher : ISubstitutionCipher
{
    public string Encrypt(string normalizedPlaintext, SubstitutionKey key)
        => Transform(normalizedPlaintext, key.Forward);

    public string Decrypt(string normalizedCiphertext, SubstitutionKey key)
        => Transform(normalizedCiphertext, key.Reverse);

    private static string Transform(string input, IReadOnlyDictionary<char, char> map)
    {
        ArgumentNullException.ThrowIfNull(input);
        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (!map.TryGetValue(c, out var m))
                throw new InvalidDataException($"Character '{c}' not present in key.");
            sb.Append(m);
        }

        return sb.ToString();
    }
}