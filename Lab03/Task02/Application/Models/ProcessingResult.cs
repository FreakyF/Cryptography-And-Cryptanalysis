namespace Task02.Application.Models;

public readonly record struct ProcessingResult(
    int ExitCode,
    string? Message
)
{
    /// <summary>Indicates whether the process completed successfully by checking if the exit code equals zero.</summary>
    public bool IsSuccess => ExitCode == 0;
}