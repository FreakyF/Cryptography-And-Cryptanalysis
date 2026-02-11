using Task02.Domain.Enums;

namespace Task02.Application.Models;

public sealed class AppOptions
{
    public string? InputPath { get; init; }
    public string? OutputPath { get; init; }
    public string? KeyPath { get; init; }
    public OperationMode Mode { get; init; } = OperationMode.Unspecified;
    public bool ShowHelp { get; init; }
    public string? G1OutputPath { get; init; }
    public string? G2OutputPath { get; init; }
    public string? G3OutputPath { get; init; }
    public string? G4OutputPath { get; init; }

    public bool AnyNGramRequested =>
        !string.IsNullOrWhiteSpace(G1OutputPath) ||
        !string.IsNullOrWhiteSpace(G2OutputPath) ||
        !string.IsNullOrWhiteSpace(G3OutputPath) ||
        !string.IsNullOrWhiteSpace(G4OutputPath);
}