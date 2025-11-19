namespace Task04.Domain.Models;

public sealed record PerformanceMetrics(
    string Algorithm,
    int? MinIterations,
    double? MeanTimeMs,
    double? MeanTextAccuracy);