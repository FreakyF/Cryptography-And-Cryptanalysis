using System.Numerics;
using System.Runtime.CompilerServices;

namespace Task02.Domain.Services;

public struct Xoshiro256
{
    private ulong _s0, _s1, _s2, _s3;

    /// <summary>Rotates the provided 64-bit value left by the specified number of bits for RNG state updates.</summary>
    /// <param name="x">The value to rotate.</param>
    /// <param name="k">The number of bits to rotate.</param>
    /// <returns>The rotated 64-bit value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotL(ulong x, int k)
    {
        return BitOperations.RotateLeft(x, k);
    }

    /// <summary>Advances the SplitMix64 generator to expand a seed into high-quality 64-bit values.</summary>
    /// <param name="x">The mutable state being evolved.</param>
    /// <returns>The generated 64-bit value for seeding.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong SplitMix64(ref ulong x)
    {
        x += 0x9E3779B97F4A7C15ul;
        var z = x;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9ul;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBul;
        return z ^ (z >> 31);
    }

    /// <summary>Initializes the RNG state from a single seed by expanding it into four 64-bit lanes.</summary>
    /// <param name="seed">The initial entropy used to seed the generator.</param>
    public Xoshiro256(ulong seed)
    {
        _s0 = _s1 = _s2 = _s3 = 0;
        var sm = seed;
        _s0 = SplitMix64(ref sm);
        _s1 = SplitMix64(ref sm);
        _s2 = SplitMix64(ref sm);
        _s3 = SplitMix64(ref sm);
    }

    /// <summary>Generates the next 64-bit pseudorandom value and advances the internal state.</summary>
    /// <returns>A 64-bit unsigned pseudorandom integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong Next64()
    {
        var s0 = _s0;
        var s1 = _s1;
        var s2 = _s2;
        var s3 = _s3;

        var result = RotL(s1 * 5, 7) * 9;

        var t = s1 << 17;

        s2 ^= s0;
        s3 ^= s1;
        s1 ^= s2;
        s0 ^= s3;

        s2 ^= t;
        s3 = RotL(s3, 45);

        _s0 = s0;
        _s1 = s1;
        _s2 = s2;
        _s3 = s3;
        return result;
    }

    /// <summary>Produces a uniformly distributed integer in the range [0, exclusiveMax).</summary>
    /// <param name="exclusiveMax">The exclusive upper bound for the generated value.</param>
    /// <returns>An integer uniformly sampled from the allowed range.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int NextInt(int exclusiveMax)
    {
        var r = Next64();
        var prod = (UInt128)r * (uint)exclusiveMax;
        return (int)(prod >> 64);
    }

    /// <summary>Generates a double precision floating-point value uniformly distributed in [0, 1).</summary>
    /// <returns>A pseudorandom double precision value in the unit interval.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double NextDouble()
    {
        return (Next64() >> 11) * (1.0 / (1ul << 53));
    }
}