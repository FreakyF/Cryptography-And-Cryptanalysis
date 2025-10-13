using Task01.Domain;

namespace Task01.Application.Abstractions;

public interface IKeyLoader
{
    SubstitutionKey Load(string path);
}