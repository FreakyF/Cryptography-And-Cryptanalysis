using Task01.Application.Services;

namespace Task01.Application.Abstractions;

public interface IArgumentParser
{
    /// <summary>Parses the provided command-line tokens into a structured argument set for the cipher application.</summary>
    /// <param name="args">The raw command-line arguments to interpret.</param>
    /// <returns>The parsed and validated arguments describing the desired operation.</returns>
    Arguments Parse(string[] args);
}