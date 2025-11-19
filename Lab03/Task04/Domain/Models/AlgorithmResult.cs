namespace Task04.Domain.Models;

public sealed record AlgorithmResult(
    string DecryptedText,
    string? RecoveredKey
);