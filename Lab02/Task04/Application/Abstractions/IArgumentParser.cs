using Task04.Application.Services;

namespace Task04.Application.Abstractions;

public interface IArgumentParser
{
    Arguments Parse(string[] args);
}