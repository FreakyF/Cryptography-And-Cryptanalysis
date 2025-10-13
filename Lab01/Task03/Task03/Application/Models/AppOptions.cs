using Task02.Domain.Enums;

namespace Task02.Application.Models;

public sealed class AppOptions
{
    public string? InputPath { get; init; }

    public string? OutputPath { get; init; }
    public string? KeyPath { get; init; }
    public OperationMode Mode { get; init; } = OperationMode.Unspecified;

    public string? G1OutputPath { get; init; }
    public string? G2OutputPath { get; init; }
    public string? G3OutputPath { get; init; }
    public string? G4OutputPath { get; init; }

    public bool ComputeChiSquare { get; init; }
    public string? R1Path { get; init; }
    public string? R2Path { get; init; }
    public string? R3Path { get; init; }
    public string? R4Path { get; init; }

    public bool ShowHelp { get; init; }

    public bool AnyNGramRequested =>
        !string.IsNullOrWhiteSpace(G1OutputPath) ||
        !string.IsNullOrWhiteSpace(G2OutputPath) ||
        !string.IsNullOrWhiteSpace(G3OutputPath) ||
        !string.IsNullOrWhiteSpace(G4OutputPath);

    public int? ReferenceOrder =>
        !string.IsNullOrWhiteSpace(R1Path) ? 1 :
        !string.IsNullOrWhiteSpace(R2Path) ? 2 :
        !string.IsNullOrWhiteSpace(R3Path) ? 3 :
        !string.IsNullOrWhiteSpace(R4Path) ? 4 :
        null;

    public string? ReferencePath =>
        ReferenceOrder switch
        {
            1 => R1Path,
            2 => R2Path,
            3 => R3Path,
            4 => R4Path,
            _ => null
        };
}