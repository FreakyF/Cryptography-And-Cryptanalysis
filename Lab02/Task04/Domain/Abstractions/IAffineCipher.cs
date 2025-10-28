namespace Task04.Domain.Abstractions;

public interface IAffineCipher
{
    string Encrypt(string normalizedText, string alphabet, int a, int b);
    string Decrypt(string normalizedText, string alphabet, int a, int b);
}