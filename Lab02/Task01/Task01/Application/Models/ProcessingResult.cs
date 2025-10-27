namespace Task01.Application.Models;

public readonly record struct ProcessingResult(
    int ExitCode,
    string? Message
);