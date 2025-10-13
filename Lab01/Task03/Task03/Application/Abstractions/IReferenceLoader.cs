namespace Task03.Application.Abstractions;

public interface IReferenceLoader
{
    NGramReference Load(string path);
}