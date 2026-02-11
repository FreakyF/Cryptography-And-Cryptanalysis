using System.IO;
using Lab06.Domain.Cryptanalysis;
using Lab06.Domain.Generators;
using Lab06.Infrastructure.Utils;

namespace Lab06.Application.Runners;

/// <summary>
/// Executes experiments to evaluate the performance and accuracy of the cryptanalysis attacks.
/// </summary>
public static class ExperimentRunner
{
    /// <summary>
    /// Runs a series of correlation attack experiments with varying keystream lengths.
    /// </summary>
    /// <remarks>
    /// Measures success rates and average execution times for different keystream lengths.
    /// Suppresses console output during the attack phase to keep the experiment log clean.
    /// </remarks>
    public static void RunExperiments()
    {
        Console.WriteLine("\n=== RUNNING EXPERIMENTS ===");

        int[] lengths = [8, 16, 24, 31, 62, 93];
        var rnd = new Random();
        var attacker = new AttackService();

        foreach (var len in lengths)
        {
            var successes = 0;
            const int trials = 20;
            double totalTime = 0;

            var originalOut = Console.Out;

            for (var i = 0; i < trials; i++)
            {
                var kX = BitUtils.IntToBinaryArray(rnd.Next(1, 1 << 3), 3);
                var kY = BitUtils.IntToBinaryArray(rnd.Next(1, 1 << 4), 4);
                var kZ = BitUtils.IntToBinaryArray(rnd.Next(1, 1 << 5), 5);

                var gen = new CombinationGenerator(
                    new Lfsr(3, [0, 1], kX),
                    new Lfsr(4, [0, 3], kY),
                    new Lfsr(5, [0, 2], kZ)
                );

                var keystream = new int[len];
                for (var b = 0; b < len; b++)
                {
                    keystream[b] = gen.NextBit();
                }

                var sw = Stopwatch.StartNew();

                try
                {
                    Console.SetOut(TextWriter.Null);

                    var result = attacker.CorrelationAttack(keystream);

                    sw.Stop();
                    totalTime += sw.Elapsed.TotalMilliseconds;

                    if (Enumerable.SequenceEqual(result.StateX, kX) &&
                        Enumerable.SequenceEqual(result.StateY, kY) &&
                        Enumerable.SequenceEqual(result.StateZ, kZ))
                    {
                        successes++;
                    }
                }
                catch
                {
                    sw.Stop();
                }
                finally
                {
                    Console.SetOut(originalOut);
                }
            }

            Console.WriteLine(
                $"Length {len} bits: Success Rate {successes}/{trials} ({(double)successes / trials:P0}), Avg Time: {totalTime / trials:F4}ms");
        }
    }
}
