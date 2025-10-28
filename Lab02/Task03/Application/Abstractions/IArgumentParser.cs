using Task03.Application.Services;

namespace Task03.Application.Abstractions;

public interface IArgumentParser
{
    Arguments Parse(string[] args);
}