using Task02.Application.Models;

namespace Task02.Application.Services;

public sealed record Arguments(
    Mode Mode,
    string KeyFilePath,
    string InputFilePath,
    string OutputFilePath
);