namespace Task03.Domain.Abstractions;

public interface IConfigurableIterations
{
    /// <summary>Configures how many iterations the heuristic search algorithm should perform.</summary>
    /// <param name="iterations">The desired number of search iterations to execute.</param>
    void SetIterations(int iterations);
}