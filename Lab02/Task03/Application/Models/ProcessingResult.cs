namespace Task03.Application.Models;

public readonly record struct ProcessingResult(
    int ExitCode,
    string? Message
)
{
    /// <summary>Indicates whether the processing ended successfully based on the exit code.</summary>
    public bool IsSuccess => ExitCode == 0;
}