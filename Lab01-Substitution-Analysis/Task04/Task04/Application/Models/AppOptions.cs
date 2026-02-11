using Task04.Domain.Enums;

namespace Task04.Application.Models;

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

    public string? B1OutputPath { get; init; }
    public string? B2OutputPath { get; init; }
    public string? B3OutputPath { get; init; }
    public string? B4OutputPath { get; init; }

    public bool ComputeChiSquare { get; init; }
    public string? R1Path { get; init; }
    public string? R2Path { get; init; }
    public string? R3Path { get; init; }
    public string? R4Path { get; init; }


    public int? SampleLength { get; init; }
    public double? MinExpected { get; init; }
    public string? ExcludeCsv { get; init; }

    public bool ShowHelp { get; init; }

    public bool AnyNGramRequested =>
        !string.IsNullOrWhiteSpace(G1OutputPath) ||
        !string.IsNullOrWhiteSpace(G2OutputPath) ||
        !string.IsNullOrWhiteSpace(G3OutputPath) ||
        !string.IsNullOrWhiteSpace(G4OutputPath);

    public bool AnyRefBuildRequested =>
        !string.IsNullOrWhiteSpace(B1OutputPath) ||
        !string.IsNullOrWhiteSpace(B2OutputPath) ||
        !string.IsNullOrWhiteSpace(B3OutputPath) ||
        !string.IsNullOrWhiteSpace(B4OutputPath);

    public int? ReferenceOrder
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(R1Path)) return 1;
            if (!string.IsNullOrWhiteSpace(R2Path)) return 2;
            if (!string.IsNullOrWhiteSpace(R3Path)) return 3;
            if (!string.IsNullOrWhiteSpace(R4Path)) return 4;
            return null;
        }
    }

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