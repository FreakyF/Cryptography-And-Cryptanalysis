namespace Task02.Domain.Abstractions;

public interface IConfigurableIterations
{
    /// <summary>Sets the number of iterations the heuristic search should perform, falling back to a default when invalid.</summary>
    /// <param name="iterations">The desired iteration count; values less than or equal to zero will be ignored.</param>
    void SetIterations(int iterations);
}