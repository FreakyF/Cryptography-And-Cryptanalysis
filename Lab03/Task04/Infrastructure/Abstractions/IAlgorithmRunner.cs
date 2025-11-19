using Task04.Domain.Models;

namespace Task04.Infrastructure.Abstractions;

public interface IAlgorithmRunner
{
    Task<AlgorithmResult> RunAsync(string cipherPath, string bigramsPath, string workDir);
}