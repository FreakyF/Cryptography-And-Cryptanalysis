namespace Task04.Application.Models;

public sealed class ChiSquareOptions(ISet<string> exclude, double? minExpected)
{
    public ISet<string> Exclude { get; } = exclude ?? throw new ArgumentNullException(nameof(exclude));
    public double? MinExpected { get; } = minExpected;
}