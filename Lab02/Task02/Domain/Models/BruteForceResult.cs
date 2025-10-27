namespace Task02.Domain.Models;

public readonly record struct BruteForceResult(
    string Plaintext,
    int Key,
    double ChiSquare,
    bool LooksEnglish
);