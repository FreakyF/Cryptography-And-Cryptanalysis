using Task01.Application.Models;

namespace Task01.Application.Services;

public sealed record Arguments(
    Mode Mode,
    string KeyFilePath,
    string InputFilePath,
    string OutputFilePath
);