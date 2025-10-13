namespace Task04.Application.Abstractions;

public interface IReferenceLoader
{
    NGramReference Load(string path);
}