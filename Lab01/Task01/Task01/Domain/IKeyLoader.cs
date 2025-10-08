namespace Task01.Domain;

public interface IKeyLoader
{
    Dictionary<char, char> Load(string path);
}