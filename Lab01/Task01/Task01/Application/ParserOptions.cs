using CommandLine;
using JetBrains.Annotations;

namespace Task01.Application;

public class ParserOptions
{
    [UsedImplicitly]
    [Option('i', "input", Required = true, HelpText = "Input file.")]
    public string? Input { get; set; }

    [UsedImplicitly]
    [Option('o', "output", Required = true, HelpText = "Output file.")]
    public string? Output { get; set; }

    [UsedImplicitly]
    [Option('k', "key", Required = true, HelpText = "Key file.")]
    public string? Key { get; set; }

    [UsedImplicitly]
    [Option('e', "encrypt", Default = false, HelpText = "Encrypt mode.")]
    public bool Encrypt { get; set; }

    [UsedImplicitly]
    [Option('d', "decrypt", Default = false, HelpText = "Decrypt mode.")]
    public bool Decrypt { get; set; }
}