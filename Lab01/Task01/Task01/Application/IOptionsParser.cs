namespace Task01.Application;

public interface IOptionsParser
{
    CommandLineOptions Parse(string[] args);
}