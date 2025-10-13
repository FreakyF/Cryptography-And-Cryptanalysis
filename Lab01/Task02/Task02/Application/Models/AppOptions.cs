using Task02.Domain.Enums;

namespace Task02.Application.Models;

public sealed class AppOptions
{
    public string? InputPath { get; init; }
    public string? OutputPath { get; init; }
    public string? KeyPath { get; init; }
    public OperationMode Mode { get; init; } = OperationMode.Unspecified;
    public bool ShowHelp { get; init; }
}