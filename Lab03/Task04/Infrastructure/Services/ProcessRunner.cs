using System.Diagnostics;
using Task04.Infrastructure.Abstractions;

namespace Task04.Infrastructure.Services;

public sealed class ProcessRunner : IProcessRunner
{
    public async Task<(int code, string stdout, string stderr)> RunAsync(
        string fileName, string arguments, string workingDir, TimeSpan? timeout = null)
    {
        var si = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = string.IsNullOrEmpty(workingDir) ? Environment.CurrentDirectory : workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var p = new Process();
        p.StartInfo = si;

        var cts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : new CancellationTokenSource();
        try
        {
            p.Start();

            var stdOutTask = p.StandardOutput.ReadToEndAsync(cts.Token);
            var stdErrTask = p.StandardError.ReadToEndAsync(cts.Token);

            await p.WaitForExitAsync(cts.Token).ConfigureAwait(false);

            var stdout = await stdOutTask.ConfigureAwait(false);
            var stderr = await stdErrTask.ConfigureAwait(false);

            return (p.ExitCode, stdout, stderr);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!p.HasExited)
                {
                    p.Kill(true);
                }
            }
            catch
            {
                /* ignore */
            }

            throw;
        }
        finally
        {
            cts.Dispose();
        }
    }
}