using Task03.Domain;

namespace Task03.Application.Abstractions;

public interface IKeyLoader
{
    SubstitutionKey Load(string path);
}