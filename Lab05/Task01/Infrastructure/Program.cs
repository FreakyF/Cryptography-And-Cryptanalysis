using System.Diagnostics.CodeAnalysis;
using Task01.Application;

namespace Task01.Infrastructure;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
public static class Program
{
    public static void Main(string[] args)
    {
        var quiet =
            args.Length > 0 &&
            string.Equals(args[0], "quiet", StringComparison.OrdinalIgnoreCase);

        IRunner runner = new Runner(quiet);
        runner.RunAll();
    }
}