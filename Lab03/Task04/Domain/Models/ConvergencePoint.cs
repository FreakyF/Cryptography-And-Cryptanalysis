namespace Task04.Domain.Models;

public sealed record ConvergencePoint(
    int Iterations,
    double MeanObjective,
    double StdObjective,
    double MeanTextAcc,
    double StdTextAcc);