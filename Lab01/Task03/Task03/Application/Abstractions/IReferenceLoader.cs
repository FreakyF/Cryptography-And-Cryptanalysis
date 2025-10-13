namespace Task02.Application.Abstractions;

public interface IReferenceLoader
{
    NGramReference Load(string path);
}