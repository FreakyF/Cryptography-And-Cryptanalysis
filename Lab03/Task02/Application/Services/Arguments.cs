using Task02.Application.Models;

namespace Task02.Application.Services;

public sealed record Arguments(
    Operation Operation,
    string InputFilePath,
    string OutputFilePath
);