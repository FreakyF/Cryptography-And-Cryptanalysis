using Task01.Application.Models;

namespace Task01.Application.Services;

public sealed record Arguments(
    Operation Operation,
    string KeyFilePath,
    string InputFilePath,
    string OutputFilePath
);