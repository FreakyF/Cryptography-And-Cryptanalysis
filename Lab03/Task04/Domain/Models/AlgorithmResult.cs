namespace Task04.Domain.Models;

public sealed record AlgorithmResult(
    string Name,
    string DecryptedText,
    string? RecoveredKey)
{
    public double ElapsedMilliseconds { get; init; }
}