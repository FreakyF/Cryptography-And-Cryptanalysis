using System.Numerics;
using System.Runtime.CompilerServices;

namespace Task03.Domain.Services;

/// <summary>Implements the xoshiro256** pseudorandom number generator for deterministic randomness.</summary>
public struct Xoshiro256
{
    private ulong _s0, _s1, _s2, _s3;

    /// <summary>Rotates the provided 64-bit integer left by the specified number of bits.</summary>
    /// <param name="x">The value to rotate.</param>
    /// <param name="k">The number of bits to rotate left.</param>
    /// <returns>The rotated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotL(ulong x, int k)
    {
        return BitOperations.RotateLeft(x, k);
    }

    /// <summary>Generates a new 64-bit value using the SplitMix64 algorithm for seeding purposes.</summary>
    /// <param name="x">The mutable state used to produce sequential values.</param>
    /// <returns>The next SplitMix64 output.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong SplitMix64(ref ulong x)
    {
        x += 0x9E3779B97F4A7C15ul;
        var z = x;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9ul;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBul;
        return z ^ (z >> 31);
    }

    /// <summary>Initializes the generator with a 64-bit seed value.</summary>
    /// <param name="seed">The seed used to derive the initial state.</param>
    public Xoshiro256(ulong seed)
    {
        _s0 = _s1 = _s2 = _s3 = 0;
        var sm = seed;
        _s0 = SplitMix64(ref sm);
        _s1 = SplitMix64(ref sm);
        _s2 = SplitMix64(ref sm);
        _s3 = SplitMix64(ref sm);
    }

    /// <summary>Produces the next 64-bit pseudorandom value and advances the internal state.</summary>
    /// <returns>A pseudorandom 64-bit unsigned integer.</returns>
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

    /// <summary>Returns a non-negative random integer less than the specified exclusive upper bound.</summary>
    /// <param name="exclusiveMax">The exclusive upper bound for the result.</param>
    /// <returns>An integer in the range [0, exclusiveMax).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int NextInt(int exclusiveMax)
    {
        var r = Next64();
        var prod = (UInt128)r * (uint)exclusiveMax;
        return (int)(prod >> 64);
    }

    /// <summary>Returns a pseudorandom double uniformly distributed in the interval [0,1).</summary>
    /// <returns>A double precision floating-point value in [0,1).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double NextDouble()
    {
        return (Next64() >> 11) * (1.0 / (1ul << 53));
    }
}