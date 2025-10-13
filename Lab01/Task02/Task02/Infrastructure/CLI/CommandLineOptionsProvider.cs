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
            ["-h"] = "help", ["--help"] = "help"
        };

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

    private static string[] NormalizeBooleanSwitches(string[] args)
    {
        var list = new List<string>(args.Length);
        for (var i = 0; i < args.Length; i++)
        {
            var t = args[i];
            var nextIsValue = NextIsValue(args, i);

            list.Add((t, nextIsValue) switch
            {
                ("-e" or "--encrypt", false) => "--encrypt=true",
                ("-d" or "--decrypt", false) => "--decrypt=true",
                ("-h" or "--help", _) => "--help=true",
                _ => t
            });
        }

        return list.ToArray();

        static bool NextIsValue(string[] a, int idx) =>
            idx + 1 < a.Length && !a[idx + 1].StartsWith('-');
    }

    private static bool ParseBool(string? value) =>
        bool.TryParse(value, out var b) && b;
}