namespace Task04.Domain.Models;

public sealed record ConvergenceSeries(string Algorithm, IReadOnlyList<ConvergencePoint> Points);