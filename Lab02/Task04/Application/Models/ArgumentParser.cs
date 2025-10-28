using Task04.Application.Abstractions;
using Task04.Application.Services;

namespace Task04.Application.Models;

public sealed class ArgumentParser : IArgumentParser
{
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

    private static Operation ResolveExclusive(Operation? current, Operation next)
    {
        if (current is null)
        {
            return next;
        }

        return current == next ? current.Value : throw new ArgumentException("Conflicting operation flags");
    }

    private static string ReadNext(string[] args, ref int index, string flag)
    {
        index++;
        if (index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
        {
            throw new ArgumentException("Missing value for " + flag);
        }

        return args[index];
    }

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

        return new Arguments(op.Value, keyPath, inputPath, outputPath);
    }
}