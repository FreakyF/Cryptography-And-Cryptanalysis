namespace Task04.Domain.Models;

public sealed record QualityMetrics(
    string Algorithm,
    double TextAccuracyPercent,
    double? KeyAccuracyPercent
);