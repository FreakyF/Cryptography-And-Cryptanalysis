using Microsoft.Extensions.Configuration;
using Task01.Application.Abstractions;
using Task01.Application.Models;
using Task01.Domain.Enums;

namespace Task01.Infrastructure.CLI;

public sealed class CommandLineOptionsProvider : IAppOptionsProvider
{
    private static readonly Dictionary<string, string> SwitchMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["-i"] = "input", ["--input"] = "input",
            ["-o"] = "output", ["--output"] = "output",
            ["-k"] = "key", ["--key"] = "key",
            ["-e"] = "encrypt", ["--encrypt"] = "encrypt",
            ["-d"] = "decrypt", ["--decrypt"] = "decrypt",
            ["-h"] = "help", ["--help"] = "help"
        };

    /// <summary>Parses command line arguments into application options while collecting parsing errors.</summary>
    /// <param name="args">The command line arguments provided by the user.</param>
    /// <param name="options">When successful, receives the populated application options.</param>
    /// <param name="errors">Receives any parsing error messages encountered during processing.</param>
    /// <returns><see langword="true"/> if options were parsed without errors; otherwise <see langword="false"/>.</returns>
    public bool TryGetOptions(string[] args, out AppOptions options, out List<string> errors)
    {
        errors = [];

        var normalized = NormalizeBooleanSwitches(args);

        IConfiguration config = new ConfigurationBuilder()
            .AddCommandLine(normalized, SwitchMap)
            .Build();

        var input = config["input"];
        var output = config["output"];
        var key = config["key"];

        var encryptFlag = ParseBool(config["encrypt"]);
        var decryptFlag = ParseBool(config["decrypt"]);
        var helpFlag = ParseBool(config["help"]);

        var mode = OperationMode.Unspecified;
        switch (encryptFlag)
        {
            case true when decryptFlag:
                errors.Add("Cannot use -e and -d together.");
                break;
            case true:
                mode = OperationMode.Encrypt;
                break;
            default:
            {
                if (decryptFlag)
                    mode = OperationMode.Decrypt;
                break;
            }
        }

        options = new AppOptions
        {
            InputPath = input,
            OutputPath = output,
            KeyPath = key,
            Mode = mode,
            ShowHelp = helpFlag
        };

        return errors.Count == 0;
    }

    /// <summary>Normalizes boolean command line switches to ensure configuration parsing receives explicit values.</summary>
    /// <param name="args">The raw command line arguments provided to the program.</param>
    /// <returns>An array of arguments where boolean switches are rewritten to include explicit assignments.</returns>
    private static string[] NormalizeBooleanSwitches(string[] args)
    {
        var list = args.Select((t, i) => t switch
        {
            "-h" or "--help" => "--help=true",
            "-e" or "--encrypt" when !NextIsValue(args, i) => "--encrypt=true",
            "-d" or "--decrypt" when !NextIsValue(args, i) => "--decrypt=true",
            _ => t
        }).ToList();

        return list.ToArray();

        static bool NextIsValue(string[] a, int idx) =>
            idx + 1 < a.Length && !a[idx + 1].StartsWith('-');
    }

    /// <summary>Parses the provided string as a boolean value and defaults to false when parsing fails.</summary>
    /// <param name="value">The string representation of a boolean flag.</param>
    /// <returns><see langword="true"/> when the value parses as true; otherwise <see langword="false"/>.</returns>
    private static bool ParseBool(string? value) =>
        bool.TryParse(value, out var b) && b;
}