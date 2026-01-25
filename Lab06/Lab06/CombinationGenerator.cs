namespace Lab06;

public class CombinationGenerator(ILfsr x, ILfsr y, ILfsr z) : IStreamGenerator
{
    public int NextBit()
    {
        var x1 = x.NextBit();
        var y1 = y.NextBit();
        var z1 = z.NextBit();

        return (x1 & y1) ^ (y1 & z1) ^ z1;
    }

    public void Reset(int[] stateX, int[] stateY, int[] stateZ)
    {
        x.Reset(stateX);
        y.Reset(stateY);
        z.Reset(stateZ);
    }
}