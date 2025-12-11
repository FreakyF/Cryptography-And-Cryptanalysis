namespace Task01;

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