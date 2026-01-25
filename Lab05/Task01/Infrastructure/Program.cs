using System.Diagnostics.CodeAnalysis;
using Task01.Application;

namespace Task01.Infrastructure;

/// <summary>
/// The entry point for the application.
/// </summary>
[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
public static class Program
{
    /// <summary>
    /// The main entry point method.
    /// </summary>
    /// <param name="args">Command-line arguments. Pass "quiet" to suppress output.</param>
    public static void Main(string[] args)
    {
        var quiet =
            args.Length > 0 &&
            string.Equals(args[0], "quiet", StringComparison.OrdinalIgnoreCase);

        IRunner runner = new Runner(quiet);
        runner.RunAll();
    }
}
