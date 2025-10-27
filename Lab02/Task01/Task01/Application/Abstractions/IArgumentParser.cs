using Task01.Application.Services;

namespace Task01.Application.Abstractions;

public interface IArgumentParser
{
    Arguments Parse(string[] args);
}