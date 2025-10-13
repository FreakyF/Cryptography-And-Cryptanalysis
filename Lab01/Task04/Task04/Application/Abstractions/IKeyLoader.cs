using Task04.Domain;

namespace Task04.Application.Abstractions;

public interface IKeyLoader
{
    SubstitutionKey Load(string path);
}