namespace Task01;

public interface ILfsr
{
    int Degree { get; }
    IReadOnlyList<bool> FeedbackCoefficients { get; }
    IReadOnlyList<bool> State { get; }
    bool NextBit();
    void Reset(IEnumerable<bool> state);
    IReadOnlyList<bool> GenerateBits(int count);
}