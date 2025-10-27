using Task01.Application.Abstractions;
using Task01.Application.Services;

namespace Task01.Application.Models;

public sealed class ArgumentParser : IArgumentParser
{
    public Arguments Parse(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            throw new ArgumentException("Missing arguments");
        }

        Operation? mode = null;
        string? keyPath = null;
        string? inputPath = null;
        string? outputPath = null;

        for (var i = 0; i < args.Length; i++)
        {
            var token = args[i];

            switch (token)
            {
                case "-e":
                case "-d":
                    mode = ResolveMode(token, mode);
                    break;

                case "-k":
                    keyPath = ReadValue(args, ref i, "-k");
                    break;

                case "-i":
                    inputPath = ReadValue(args, ref i, "-i");
                    break;

                case "-o":
                    outputPath = ReadValue(args, ref i, "-o");
                    break;

                default:
                    throw new ArgumentException("Unknown argument " + token);
            }
        }

        return BuildArguments(mode, keyPath, inputPath, outputPath);
    }

    private static Operation ResolveMode(string flag, Operation? current)
    {
        var next = flag == "-e" ? Operation.Encrypt : Operation.Decrypt;

        if (current is null)
        {
            return next;
        }

        return current == next ? current.Value : throw new ArgumentException("Flags -e and -d cannot be used together");
    }

    private static string ReadValue(string[] args, ref int index, string flag)
    {
        index++;
        if (index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
        {
            throw new ArgumentException("Missing value for " + flag);
        }

        return args[index];
    }

    private static Arguments BuildArguments(Operation? mode, string? keyPath, string? inputPath, string? outputPath)
    {
        if (mode is null)
        {
            throw new ArgumentException("Missing -e or -d");
        }

        if (string.IsNullOrWhiteSpace(keyPath))
        {
            throw new ArgumentException("Missing -k <keyfile>");
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
            keyPath,
            inputPath,
            outputPath
        );
    }
}