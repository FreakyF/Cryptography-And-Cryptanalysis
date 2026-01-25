namespace Lab06;

public interface IStreamGenerator
{
    int NextBit();
    void Reset(int[] stateX, int[] stateY, int[] stateZ);
}