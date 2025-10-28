namespace Task04.Application.Models;

public readonly record struct ProcessingResult(
    int ExitCode,
    string? Message
)
{
    /// <summary>Indicates whether the processing finished without errors.</summary>
    public bool IsSuccess => ExitCode == 0;
}