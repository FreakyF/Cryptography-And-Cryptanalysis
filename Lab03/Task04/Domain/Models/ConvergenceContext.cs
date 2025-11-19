namespace Task04.Domain.Models;

public sealed record ConvergenceContext(
    string CipherPath,
    string BigramsPath,
    string WorkDir,
    string ReferenceNormalized);