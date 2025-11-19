namespace Task03.Application.Models;

public sealed record Arguments(
    Operation Operation,
    string InputFilePath,
    string OutputFilePath,
    string? ReferenceFilePath
);