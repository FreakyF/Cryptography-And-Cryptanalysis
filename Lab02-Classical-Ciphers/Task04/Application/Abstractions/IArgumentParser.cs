using Task04.Application.Services;

namespace Task04.Application.Abstractions;

public interface IArgumentParser
{
    /// <summary>Validates and converts the provided command-line tokens into structured cipher arguments.</summary>
    /// <param name="args">The command-line tokens describing the desired operation and file paths.</param>
    /// <returns>The arguments object containing the parsed settings.</returns>
    Arguments Parse(string[] args);
}