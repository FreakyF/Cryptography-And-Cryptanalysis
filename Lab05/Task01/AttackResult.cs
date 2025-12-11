namespace Task01;

public sealed record AttackResult(
    IReadOnlyList<bool> FeedbackCoefficients,
    IReadOnlyList<bool> InitialState,
    IReadOnlyList<bool> KeyStream);