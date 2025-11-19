namespace Task03.Application.Models;

/// <summary>Represents the outcome of executing a cipher operation along with exit metadata.</summary>
/// <param name="ExitCode">The exit code that should be returned to the operating system.</param>
/// <param name="Message">Optional diagnostic text describing an error.</param>
public readonly record struct ProcessingResult(
    int ExitCode,
    string? Message
)
{
    /// <summary>Gets a value indicating whether the operation completed successfully.</summary>
    public bool IsSuccess => ExitCode == 0;
}