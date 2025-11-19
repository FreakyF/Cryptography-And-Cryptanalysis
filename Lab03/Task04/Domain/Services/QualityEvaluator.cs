using System.Runtime.CompilerServices;
using Task04.Domain.Abstractions;

namespace Task04.Domain.Services;

public sealed class QualityEvaluator(ITextNormalizer normalizer) : IQualityEvaluator
{
    public (double textAcc, double? keyAcc) Evaluate(
        string decrypted, string reference, string? recoveredKey, string? trueKey)
    {
        var dec = normalizer.Normalize(decrypted);
        var refn = normalizer.Normalize(reference);

        var n = Math.Min(dec.Length, refn.Length);
        var ok = 0;
        for (var i = 0; i < n; i++)
        {
            if (dec[i] == refn[i])
            {
                ok++;
            }
        }

        var textAcc = n == 0 ? 0 : 100.0 * ok / n;
        var keyAcc = ComputeKeyAccuracy(recoveredKey, trueKey);

        return (textAcc, keyAcc);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double? ComputeKeyAccuracy(string? recoveredKey, string? trueKey)
    {
        if (string.IsNullOrWhiteSpace(recoveredKey) || string.IsNullOrWhiteSpace(trueKey))
        {
            return null;
        }

        var rk = normalizer.Normalize(recoveredKey);
        var tk = normalizer.Normalize(trueKey);
        if (rk.Length != 26 || tk.Length != 26)
        {
            return null;
        }

        var hit = 0;
        for (var i = 0; i < 26; i++)
        {
            if (rk[i] == tk[i])
            {
                hit++;
            }
        }

        return 100.0 * hit / 26.0;
    }
}