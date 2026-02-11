using Task01.Application;
using Task01.Domain.Core;

namespace Task01;

/// <summary>
///     Entry point for the Lab07/Task01 application.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Main execution method. Initializes the Trivium cipher and runs the defined experimental suite.
    /// </summary>
    /// <param name="args">Command-line arguments (unused).</param>
    private static void Main(string[] args)
    {
        // Instantiate the core cipher engine
        ITriviumCipher cipher = new TriviumCipher();

        // Initialize the experiment runner with the cipher instance
        var runner = new ExperimentRunner(cipher);

        // Execute experiments sequentially
        runner.RunExperiment1Verification();
        runner.RunExperiment2IvReuse();
        runner.RunExperiment3RoundsAnalysis();
        runner.RunExperiment4CubeAttack();
        runner.RunExperiment5Statistics();
        runner.RunExperiment6HighVolumeThroughput();
    }
}
