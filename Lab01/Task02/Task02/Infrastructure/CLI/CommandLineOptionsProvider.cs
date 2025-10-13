using Microsoft.Extensions.Configuration;
using Task02.Application.Abstractions;
using Task02.Application.Models;
using Task02.Domain.Enums;

namespace Task02.Infrastructure.CLI;

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
            ["-h"] = "help", ["--help"] = "help",
            ["-g1"] = "g1", ["--g1"] = "g1",
            ["-g2"] = "g2", ["--g2"] = "g2",
            ["-g3"] = "g3", ["--g3"] = "g3",
            ["-g4"] = "g4", ["--g4"] = "g4"
        };

    public bool TryGetOptions(string[] args, out AppOptions options, out List<string> errors)
    {
        errors = [];

        var normalized = NormalizeBooleanSwitches(args); // tylko -e/-d/-h
        IConfiguration config = new ConfigurationBuilder()
            .AddCommandLine(normalized, SwitchMap)
            .Build();

        var input = config["input"];
        var output = config["output"];
        var key = config["key"];
        var g1 = config["g1"];
        var g2 = config["g2"];
        var g3 = config["g3"];
        var g4 = config["g4"];

        var encrypt = ParseBool(config["encrypt"]);
        var decrypt = ParseBool(config["decrypt"]);
        var help = ParseBool(config["help"]);

        var mode = OperationMode.Unspecified;
        switch (encrypt)
        {
            case true when decrypt:
                errors.Add("Cannot use -e and -d together.");
                break;
            case true:
                mode = OperationMode.Encrypt;
                break;
            default:
            {
                if (decrypt) mode = OperationMode.Decrypt;
                break;
            }
        }

        options = new AppOptions
        {
            InputPath = input,
            OutputPath = output,
            KeyPath = key,
            Mode = mode,
            ShowHelp = help,
            G1OutputPath = g1,
            G2OutputPath = g2,
            G3OutputPath = g3,
            G4OutputPath = g4
        };

        return errors.Count == 0;
    }

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

    private static bool ParseBool(string? value) =>
        bool.TryParse(value, out var b) && b;
}