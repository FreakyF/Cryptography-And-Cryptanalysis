namespace Lab06;

public interface ILfsr
{
    int NextBit();
    void Reset(int[] startState);
}