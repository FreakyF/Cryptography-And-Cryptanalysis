namespace Lab06.Domain.Generators;

public interface IStreamGenerator
{
    int NextBit();
    void Reset(int[] stateX, int[] stateY, int[] stateZ);
}