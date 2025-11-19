using Task02.Application.Abstractions;
using Task02.Application.Models;

namespace Task02.Application.Services;

public sealed class ArgumentParser : IArgumentParser
{
    /// <summary>Validates and converts the provided command-line tokens into structured cipher arguments.</summary>
    /// <param name="args">The command-line tokens describing the desired operation and file paths.</param>
    /// <returns>The arguments object containing the parsed settings.</returns>
    public Arguments Parse(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            throw new ArgumentException("Missing arguments");
        }

        Operation? mode = null;
        string? inputPath = null;
        string? outputPath = null;
        string? referencePath = null;

        for (var i = 0; i < args.Length; i++)
        {
            var token = args[i];

            switch (token)
            {
                case "-e":
                case "-d":
                    mode = ResolveMode(token, mode);
                    break;
                case "-i":
                    inputPath = ReadValue(args, ref i, "-i");
                    break;
                case "-o":
                    outputPath = ReadValue(args, ref i, "-o");
                    break;
                case "-r":
                    referencePath = ReadValue(args, ref i, "-r");
                    break;
                default:
                    throw new ArgumentException("Unknown argument " + token);
            }
        }

        return BuildArguments(mode, inputPath, outputPath, referencePath);
    }

    /// <summary>Determines the cipher mode based on a flag while preventing conflicting selections.</summary>
    /// <param name="flag">The mode flag currently being processed.</param>
    /// <param name="current">The mode previously selected, if any.</param>
    /// <returns>The resolved operation corresponding to the supplied flag.</returns>
    private static Operation ResolveMode(string flag, Operation? current)
    {
        var next = flag == "-e" ? Operation.Encrypt : Operation.Decrypt;

        if (current is null)
        {
            return next;
        }

        return current == next ? current.Value : throw new ArgumentException("Flags -e and -d cannot be used together");
    }

    /// <summary>Reads the value that follows a flag and advances the iteration index accordingly.</summary>
    /// <param name="args">The full command-line argument array being parsed.</param>
    /// <param name="index">The current position in the argument list, which will be incremented.</param>
    /// <param name="flag">The flag whose value is expected next.</param>
    /// <returns>The string value extracted for the specified flag.</returns>
    private static string ReadValue(string[] args, ref int index, string flag)
    {
        index++;
        if (index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
        {
            throw new ArgumentException("Missing value for " + flag);
        }

        return args[index];
    }

    /// <summary>Builds the final arguments record after verifying that all required values are present.</summary>
    /// <param name="mode">The parsed operation mode, if any was selected.</param>
    /// <param name="inputPath">The path to the input text file.</param>
    /// <param name="outputPath">The path to the output text file.</param>
    /// <returns>A fully populated arguments record ready for processing.</returns>
    private static Arguments BuildArguments(Operation? mode, string? inputPath, string? outputPath,
        string? referencePath)
    {
        if (mode is null)
        {
            throw new ArgumentException("Missing -e or -d");
        }

        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new ArgumentException("Missing -i <inputfile>");
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Missing -o <outputfile>");
        }

        return new Arguments(
            mode.Value,
            inputPath,
            outputPath,
            referencePath
        );
    }
}