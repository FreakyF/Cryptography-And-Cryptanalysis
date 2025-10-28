namespace Task04.Domain.Models;

public readonly record struct BruteForceResult(
    string Plaintext,
    int A,
    int B,
    double ChiSquare,
    bool LooksEnglish
);