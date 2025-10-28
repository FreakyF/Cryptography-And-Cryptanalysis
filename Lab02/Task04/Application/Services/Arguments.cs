using Task04.Application.Models;

namespace Task04.Application.Services;

public sealed record Arguments(
    Operation Operation,
    string? KeyFilePath,
    string InputFilePath,
    string OutputFilePath
);