namespace Lab06.Domain.Generators;

public interface ILfsr
{
    int NextBit();
    void Reset(int[] startState);
}