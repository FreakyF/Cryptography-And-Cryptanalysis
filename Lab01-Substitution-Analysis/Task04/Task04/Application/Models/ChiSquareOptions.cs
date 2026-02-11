namespace Task04.Application.Models;

/// <summary>Encapsulates optional parameters influencing chi-square analysis.</summary>
/// <param name="exclude">The set of n-grams that should be ignored during computation.</param>
/// <param name="minExpected">The minimum expected count threshold below which classes are skipped.</param>
public sealed class ChiSquareOptions(ISet<string> exclude, double? minExpected)
{
    /// <summary>Gets the set of n-grams to exclude from the chi-square summation.</summary>
    public ISet<string> Exclude { get; } = exclude ?? throw new ArgumentNullException(nameof(exclude));
    /// <summary>Gets the optional minimum expected count threshold.</summary>
    public double? MinExpected { get; } = minExpected;
}