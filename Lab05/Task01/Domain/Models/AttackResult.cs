namespace Task01.Domain.Models;

public sealed record AttackResult(
    IReadOnlyList<bool> FeedbackCoefficients,
    IReadOnlyList<bool> InitialState,
    IReadOnlyList<bool> KeyStream);