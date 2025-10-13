namespace Task01.Domain;

public sealed class SubstitutionKey(IReadOnlyDictionary<char, char> forward, IReadOnlyDictionary<char, char> reverse)
{
    public IReadOnlyDictionary<char, char> Forward { get; } =
        forward ?? throw new ArgumentNullException(nameof(forward));

    public IReadOnlyDictionary<char, char> Reverse { get; } =
        reverse ?? throw new ArgumentNullException(nameof(reverse));

    public static SubstitutionKey FromForward(IDictionary<char, char> forward)
    {
        ArgumentNullException.ThrowIfNull(forward);
        var rev = new Dictionary<char, char>(26);
        foreach (var kv in forward)
            rev[kv.Value] = kv.Key;
        return new SubstitutionKey(new Dictionary<char, char>(forward), rev);
    }
}