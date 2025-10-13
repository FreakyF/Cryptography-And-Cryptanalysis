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

            // n-gramy generowanie
            ["-g1"] = "g1", ["--g1"] = "g1",
            ["-g2"] = "g2", ["--g2"] = "g2",
            ["-g3"] = "g3", ["--g3"] = "g3",
            ["-g4"] = "g4", ["--g4"] = "g4",

            // baza referencyjna i test chi^2
            ["-r1"] = "r1", ["--r1"] = "r1",
            ["-r2"] = "r2", ["--r2"] = "r2",
            ["-r3"] = "r3", ["--r3"] = "r3",
            ["-r4"] = "r4", ["--r4"] = "r4",
            ["-s"] = "chisq", ["--chisq"] = "chisq"
        };

    public bool TryGetOptions(string[] args, out AppOptions options, out List<string> errors)
    {
        errors = [];

        var normalized = NormalizeBooleanSwitches(args); // tylko przełączniki bool
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
        var r1 = config["r1"];
        var r2 = config["r2"];
        var r3 = config["r3"];
        var r4 = config["r4"];

        var encrypt = ParseBool(config["encrypt"]);
        var decrypt = ParseBool(config["decrypt"]);
        var help = ParseBool(config["help"]);
        var chisq = ParseBool(config["chisq"]);

        var mode = OperationMode.Unspecified;
        if (encrypt && decrypt)
            errors.Add("Cannot use -e and -d together.");
        else if (encrypt) mode = OperationMode.Encrypt;
        else if (decrypt) mode = OperationMode.Decrypt;

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
            G4OutputPath = g4,

            ComputeChiSquare = chisq,
            R1Path = r1,
            R2Path = r2,
            R3Path = r3,
            R4Path = r4
        };

        return errors.Count == 0;
    }

    private static string[] NormalizeBooleanSwitches(string[] args)
    {
        var list = args
            .Select((t, i) => (t, NextIsValue(args, i)) switch
            {
                ("-e" or "--encrypt", false) => "--encrypt=true",
                ("-d" or "--decrypt", false) => "--decrypt=true",
                ("-h" or "--help", _) => "--help=true",
                ("-s" or "--chisq", _) => "--chisq=true",
                _ => t
            })
            .ToList();

        return list.ToArray();

        static bool NextIsValue(string[] a, int idx) =>
            idx + 1 < a.Length && !a[idx + 1].StartsWith('-');
    }

    private static bool ParseBool(string? value) =>
        bool.TryParse(value, out var b) && b;
}