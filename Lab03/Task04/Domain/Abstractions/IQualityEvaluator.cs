namespace Task04.Domain.Abstractions;

public interface IQualityEvaluator
{
    (double textAcc, double? keyAcc) Evaluate(string decrypted, string reference, string? recoveredKey,
        string? trueKey);
}