using Task02.Application.Services;

namespace Task02.Application.Abstractions;

public interface IArgumentParser
{
    Arguments Parse(string[] args);
}