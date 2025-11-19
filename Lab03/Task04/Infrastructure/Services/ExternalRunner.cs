using System.Diagnostics;
using Task04.Domain.Models;
using Task04.Infrastructure.Abstractions;

namespace Task04.Infrastructure.Services;

public sealed class ExternalRunner(IProcessRunner process, string exePath, string label) : IAlgorithmRunner
{
    public async Task<AlgorithmResult> RunAsync(
        string cipherPath,
        string bigramsPath,
        string workDir,
        int? iterations = null,
        CancellationToken ct = default)
    {
        var exeFull = Path.GetFullPath(exePath);
        var exeDir = Path.GetDirectoryName(exeFull) ?? Environment.CurrentDirectory;
        var cipherFull = Path.GetFullPath(cipherPath);
        var bigramsFull = Path.GetFullPath(bigramsPath);

        var workPerAlgo = Path.Combine(Path.GetFullPath(workDir), label);
        Directory.CreateDirectory(workPerAlgo);

        var outText = Path.Combine(workPerAlgo, "output.txt");
        var outKey = Path.Combine(workPerAlgo, "output_key.txt");

        TryDelete(outText);
        TryDelete(outKey);

        var itersArg = iterations.HasValue ? $" --iters {iterations.Value}" : string.Empty;
        var args = $"-d -i \"{cipherFull}\" -o \"{outText}\" -r \"{bigramsFull}\"{itersArg}";

        var sw = Stopwatch.StartNew();
        var (code, _, stderr) = await process.RunAsync(exeFull, args, exeDir).ConfigureAwait(false);
        sw.Stop();

        if (code != 0)
        {
            throw new InvalidOperationException($"{label} failed: exit={code} stderr={stderr}");
        }

        var decrypted = await File.ReadAllTextAsync(outText, ct).ConfigureAwait(false);
        var key = File.Exists(outKey)
            ? await File.ReadAllTextAsync(outKey, ct).ConfigureAwait(false)
            : null;

        return new AlgorithmResult(label, decrypted, key) { ElapsedMilliseconds = sw.Elapsed.TotalMilliseconds };
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            /* ignore */
        }
    }
}