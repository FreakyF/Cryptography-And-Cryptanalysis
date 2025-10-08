using Task01.Domain;

namespace Task01.Application;

public sealed class CommandLineOptions
{
    public string InputFile { get; init; } = string.Empty;
    public string OutputFile { get; init; } = string.Empty;
    public string KeyFile { get; init; } = string.Empty;
    public CipherMode Mode { get; init; }
}