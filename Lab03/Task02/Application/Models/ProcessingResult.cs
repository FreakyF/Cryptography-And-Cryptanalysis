namespace Task02.Application.Models;

public readonly record struct ProcessingResult(
    int ExitCode,
    string? Message
)
{
    public bool IsSuccess => ExitCode == 0;
}