using BenchmarkDotNet.Attributes;

namespace Task01;

[MemoryDiagnoser]
public class BitConversionsBenchmarks
{
    private string _text = null!;
    private IReadOnlyList<bool> _bits = null!;
    private string _bitString = null!;
    private int[] _intArray = null!;
    private IReadOnlyList<bool> _bitsFromInts = null!;
    private IReadOnlyList<int> _intsFromBits = null!;

    [GlobalSetup]
    public void Setup()
    {
        _text = "This is a sample text used to benchmark bit conversions.";
        _bits = BitConversions.StringToBits(_text);
        _bitString = BitConversions.BitsToBitString(_bits);
        _intArray = Enumerable.Repeat(0, 64).ToArray();
        for (var i = 0; i < _intArray.Length; i++)
        {
            _intArray[i] = i % 2;
        }

        _bitsFromInts = BitConversions.IntArrayToBits(_intArray);
        _intsFromBits = BitConversions.BitsToIntArray(_bits);
    }

    [Benchmark]
    public IReadOnlyList<bool> StringToBitsBenchmark()
    {
        return BitConversions.StringToBits(_text);
    }

    [Benchmark]
    public string BitsToStringBenchmark()
    {
        return BitConversions.BitsToString(_bits);
    }

    [Benchmark]
    public IReadOnlyList<bool> BitStringToBitsBenchmark()
    {
        return BitConversions.BitStringToBits(_bitString);
    }

    [Benchmark]
    public string BitsToBitStringBenchmark()
    {
        return BitConversions.BitsToBitString(_bits);
    }

    [Benchmark]
    public IReadOnlyList<bool> IntArrayToBitsBenchmark()
    {
        return BitConversions.IntArrayToBits(_intArray);
    }

    [Benchmark]
    public IReadOnlyList<int> BitsToIntArrayBenchmark()
    {
        return BitConversions.BitsToIntArray(_bits);
    }
}