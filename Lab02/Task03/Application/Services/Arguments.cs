using Task03.Application.Models;

namespace Task03.Application.Services;

public sealed record Arguments(
    Operation Operation,
    string KeyFilePath,
    string InputFilePath,
    string OutputFilePath
);