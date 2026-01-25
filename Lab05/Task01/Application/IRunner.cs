namespace Task01.Application;

/// <summary>
/// Defines the contract for the main application runner that executes verifications and experiments.
/// </summary>
public interface IRunner
{
    /// <summary>
    /// Executes all configured tasks, including component verification and experimental scenarios.
    /// </summary>
    void RunAll();
}
