using Task02.Domain;

namespace Task02.Application.Abstractions;

public interface IKeyLoader
{
    SubstitutionKey Load(string path);
}