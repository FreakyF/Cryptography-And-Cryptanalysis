namespace Task04.Application.Models;

public sealed record QualityComparisonArgs(
    string Task02ExePath,
    string Task03ExePath,
    string CipherPath,
    string BigramsPath,
    string PlainRefPath,
    string? TrueKeyPath,
    string WorkDir
);