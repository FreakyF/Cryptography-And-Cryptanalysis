namespace Task03.Application.Abstractions;

public sealed class NGramReference
{
    public int Order { get; }
    public IReadOnlyDictionary<string, double> Probabilities { get; }

    /// <summary>Initializes a reference distribution with the specified n-gram order and probability map.</summary>
    /// <param name="order">The n-gram length represented by the reference.</param>
    /// <param name="probabilities">The probability values keyed by n-gram string.</param>
    public NGramReference(int order, IReadOnlyDictionary<string, double> probabilities)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(order, 1);
        Order = order;
        Probabilities = probabilities ?? throw new ArgumentNullException(nameof(probabilities));
    }
}