namespace Task04.Infrastructure.Abstractions;

public interface IProcessRunner
{
    Task<(int code, string stdout, string stderr)> RunAsync(
        string fileName, string arguments, string workingDir, TimeSpan? timeout = null);
}