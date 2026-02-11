using Task01.Domain.Enums;

namespace Task01.Application.Models;

public sealed class AppOptions
{
    public string? InputPath { get; init; }
    public string? OutputPath { get; init; }
    public string? KeyPath { get; init; }
    public OperationMode Mode { get; init; } = OperationMode.Unspecified;
    public bool ShowHelp { get; init; }
}