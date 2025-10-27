namespace Task01.Domain.Abstractions;

public interface ICaesarCipher
{
    string Encrypt(string normalizedText, string alphabet, int key);
    string Decrypt(string normalizedText, string alphabet, int key);
}