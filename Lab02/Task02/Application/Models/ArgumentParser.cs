using Task02.Application.Abstractions;
using Task02.Application.Services;

namespace Task02.Application.Models;

public sealed class ArgumentParser : IArgumentParser
{
    /// <summary>Validates and converts the provided command-line tokens into structured arguments for encryption, decryption, or brute force.</summary>
    /// <param name="args">The command-line tokens describing the desired operation and file paths.</param>
    /// <returns>The arguments object containing the parsed settings.</returns>
    public Arguments Parse(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            throw new ArgumentException("Missing arguments");
        }

        Operation? op = null;
        string? keyPath = null;
        string? inputPath = null;
        string? outputPath = null;

        var i = 0;
        while (i < args.Length)
        {
            var token = args[i];

            switch (token)
            {
                case "-e":
                    op = ResolveExclusive(op, Operation.Encrypt);
                    break;

                case "-d":
                    op = ResolveExclusive(op, Operation.Decrypt);
                    break;

                case "-a":
                    var attackMode = ReadNext(args, ref i, "-a");
                    if (attackMode != "bf")
                    {
                        throw new ArgumentException("Unsupported attack mode " + attackMode);
                    }

                    op = ResolveExclusive(op, Operation.BruteForce);
                    break;

                case "-k":
                    keyPath = ReadNext(args, ref i, "-k");
                    break;

                case "-i":
                    inputPath = ReadNext(args, ref i, "-i");
                    break;

                case "-o":
                    outputPath = ReadNext(args, ref i, "-o");
                    break;

                default:
                    throw new ArgumentException("Unknown argument " + token);
            }

            i++;
        }

        return BuildArguments(op, keyPath, inputPath, outputPath);
    }

    /// <summary>Determines the operation while preventing conflicting choices between mutually exclusive flags.</summary>
    /// <param name="current">The operation previously selected, if any.</param>
    /// <param name="next">The operation represented by the flag currently being processed.</param>
    /// <returns>The resolved operation once exclusivity is confirmed.</returns>
    private static Operation ResolveExclusive(Operation? current, Operation next)
    {
        if (current is null)
        {
            return next;
        }

        return current == next
            ? current.Value
            : throw new ArgumentException("Conflicting operation flags");
    }

    /// <summary>Reads the value that follows a flag and advances the iteration index accordingly.</summary>
    /// <param name="args">The full command-line argument array being parsed.</param>
    /// <param name="index">The current position in the argument list, which will be incremented.</param>
    /// <param name="flag">The flag whose value is expected next.</param>
    /// <returns>The string value extracted for the specified flag.</returns>
    private static string ReadNext(string[] args, ref int index, string flag)
    {
        index++;
        if (index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
        {
            throw new ArgumentException("Missing value for " + flag);
        }

        return args[index];
    }

    /// <summary>Builds the final arguments record after verifying that all required values are present.</summary>
    /// <param name="op">The parsed operation, if any was selected.</param>
    /// <param name="keyPath">The path to the key file specified by the user.</param>
    /// <param name="inputPath">The path to the input text file.</param>
    /// <param name="outputPath">The path to the output text file.</param>
    /// <returns>A fully populated arguments record ready for processing.</returns>
    private static Arguments BuildArguments(Operation? op, string? keyPath, string? inputPath, string? outputPath)
    {
        if (op is null)
        {
            throw new ArgumentException("Missing -e or -d or -a bf");
        }

        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new ArgumentException("Missing -i <inputfile>");
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Missing -o <outputfile>");
        }

        if (op != Operation.BruteForce && string.IsNullOrWhiteSpace(keyPath))
        {
            throw new ArgumentException("Missing -k <keyfile>");
        }

        return new Arguments(
            op.Value,
            keyPath,
            inputPath,
            outputPath
        );
    }
}