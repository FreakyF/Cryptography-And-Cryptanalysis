namespace Task04.Application.Models;

public readonly record struct ProcessingResult(
    int ExitCode,
    string? Message
);