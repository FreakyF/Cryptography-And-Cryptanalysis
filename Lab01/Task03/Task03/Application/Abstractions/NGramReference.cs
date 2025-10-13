namespace Task02.Application.Abstractions;

public sealed class NGramReference
{
    public int Order { get; }
    public IReadOnlyDictionary<string, double> Probabilities { get; }

    public NGramReference(int order, IReadOnlyDictionary<string, double> probabilities)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(order, 1);
        Order = order;
        Probabilities = probabilities ?? throw new ArgumentNullException(nameof(probabilities));
    }
}