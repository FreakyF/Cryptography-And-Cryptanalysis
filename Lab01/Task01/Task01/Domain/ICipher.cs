namespace Task01.Domain;

public interface ICipher
{
    string Encrypt(string text);
    string Decrypt(string text);
}