using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Task01.Domain.Core;

[SkipLocalsInit]
public unsafe class TriviumCipher : ITriviumCipher
{
    private const int ChunkSize = 16 * 1024;

    private const int ParallelThreshold = 8 * ChunkSize;

    private static readonly Vector256<ulong> C0_Init = Vector256.Create(7UL);

    private ulong _a0, _a1, _b0, _b1, _c0, _c1;

    private ulong _baseK0, _baseK1, _baseIv0, _baseIv1;
    private ulong _singleBitBuffer;
    private int _singleBitRemaining;

    public TriviumCipher()
    {
        Array.Empty<bool>();
        Array.Empty<bool>();
    }

    public long LastWarmupTicks { get; }
    public long LastGenerationTicks { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Initialize(byte[] key, byte[] iv, int warmupRounds = 1152)
    {
        _baseK0 = 0;
        _baseK1 = 0;
        _baseIv0 = 0;
        _baseIv1 = 0;
        for (var i = 0; i < 80; i++)
        {
            if ((key[i / 8] & (1 << (i % 8))) != 0)
            {
                var p = 92 - i;
                if (p >= 64)
                {
                    _baseK1 |= 1UL << (p - 64);
                }
                else
                {
                    _baseK0 |= 1UL << p;
                }
            }

            if ((iv[i / 8] & (1 << (i % 8))) != 0)
            {
                var p = 83 - i;
                if (p >= 64)
                {
                    _baseIv1 |= 1UL << (p - 64);
                }
                else
                {
                    _baseIv0 |= 1UL << p;
                }
            }
        }

        _a0 = _baseK0;
        _a1 = _baseK1;
        _b0 = _baseIv0;
        _b1 = _baseIv1;
        _c0 = 7UL;
        _c1 = 0;

        var batches = warmupRounds / 64;
        for (var i = 0; i < batches; i++)
        {
            UpdateState64Scalar(ref _a0, ref _a1, ref _b0, ref _b1, ref _c0, ref _c1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GenerateBit()
    {
        if (_singleBitRemaining == 0)
        {
            var t1_z = ((_a0 >> 27) | (_a1 << 37)) ^ _a0;
            var t2_z = ((_b0 >> 15) | (_b1 << 49)) ^ _b0;
            var t3_z = ((_c0 >> 45) | (_c1 << 19)) ^ _c0;
            _singleBitBuffer = t1_z ^ t2_z ^ t3_z;

            UpdateState64Scalar(ref _a0, ref _a1, ref _b0, ref _b1, ref _c0, ref _c1);

            _singleBitRemaining = 64;
        }

        var b = (_singleBitBuffer & 1) != 0;
        _singleBitBuffer >>= 1;
        _singleBitRemaining--;
        return b;
    }

    public byte[] Encrypt(byte[] plaintext)
    {
        return plaintext.Length >= ParallelThreshold
            ? EncryptParallel(plaintext)
            : EncryptSequential(plaintext);
    }

    public byte[] Decrypt(byte[] ciphertext)
    {
        return Encrypt(ciphertext);
    }

    public (int OnesCount, double Balance) GetStateStatistics()
    {
        var ones = BitOperations.PopCount(_a0) + BitOperations.PopCount(_a1) +
                   BitOperations.PopCount(_b0) + BitOperations.PopCount(_b1) +
                   BitOperations.PopCount(_c0) + BitOperations.PopCount(_c1);
        return (ones, ones / 288.0);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public byte[] GenerateKeystream(int lengthInBytes)
    {
        var result = GC.AllocateUninitializedArray<byte>(lengthInBytes);

        if (lengthInBytes >= 256 * 1024)
        {
            return GenerateKeystreamBitsliced(lengthInBytes);
        }

        var sw = Stopwatch.StartNew();
        fixed (byte* resPtr = result)
        {
            var dst = (ulong*)resPtr;
            var blocks = lengthInBytes / 8;
            ulong a0 = _a0, a1 = _a1, b0 = _b0, b1 = _b1, c0 = _c0, c1 = _c1;

            for (var i = 0; i < blocks; i++)
            {
                var z = ((a0 >> 27) | (a1 << 37)) ^ a0 ^ ((b0 >> 15) | (b1 << 49)) ^ b0 ^ ((c0 >> 45) | (c1 << 19)) ^
                        c0;
                dst[i] = z;
                UpdateState64Scalar(ref a0, ref a1, ref b0, ref b1, ref c0, ref c1);
            }

            _a0 = a0;
            _a1 = a1;
            _b0 = b0;
            _b1 = b1;
            _c0 = c0;
            _c1 = c1;
        }

        sw.Stop();
        LastGenerationTicks = sw.ElapsedTicks;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private byte[] GenerateKeystreamBitsliced(int lengthInBytes)
    {
        var result = GC.AllocateUninitializedArray<byte>(lengthInBytes);
        var numStreams = 256;
        var bytesPerStream = lengthInBytes / numStreams;

        fixed (byte* ptr = result)
        {
            ProcessBitslicedBatch256(bytesPerStream);
        }

        return result;
    }

    private void ProcessBitslicedBatch256(int streamLen)
    {
        var S = new Vector256<ulong>[288];

        for (var i = 0; i < 288; i++)
        {
            S[i] = Vector256<ulong>.Zero;
        }

        for (var byteIdx = 0; byteIdx < streamLen; byteIdx++)
        {
            for (var bit = 0; bit < 8; bit++)
            {
                var t1 = S[65] ^ S[92];
                var t2 = S[161] ^ S[176];
                var t3 = S[242] ^ S[287];

                t1 ^= (S[90] & S[91]) ^ S[170];
                t2 ^= (S[174] & S[175]) ^ S[263];
                t3 ^= (S[285] & S[286]) ^ S[68];

                for (var k = 287; k > 0; k--)
                {
                    S[k] = S[k - 1];
                }

                S[0] = t3;
                S[93] = t1;
                S[177] = t2;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private byte[] EncryptParallel(byte[] data)
    {
        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var ptr = (byte*)handle.AddrOfPinnedObject();
        var len = data.Length;

        var bytesPerBatch = ChunkSize * 8;
        var tasks = (len + bytesPerBatch - 1) / bytesPerBatch;

        var sw = Stopwatch.StartNew();
        Parallel.For(0, tasks, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, t =>
        {
            var chunkIdx = t * 8;
            var off = (long)chunkIdx * ChunkSize;
            if (off + (long)ChunkSize * 8 <= len)
            {
                ProcessByteBatch8(chunkIdx, ptr);
            }
            else
            {
                ProcessByteBatchSafe(chunkIdx, len, ptr);
            }
        });
        sw.Stop();

        handle.Free();
        LastGenerationTicks = sw.ElapsedTicks;
        return data;
    }

    private byte[] EncryptSequential(byte[] plaintext)
    {
        var result = GC.AllocateUninitializedArray<byte>(plaintext.Length);
        var sw = Stopwatch.StartNew();

        fixed (byte* ptPtr = plaintext)
        fixed (byte* resPtr = result)
        {
            var pt = (ulong*)ptPtr;
            var res = (ulong*)resPtr;
            var blocks = plaintext.Length / 8;

            ulong a0 = _a0, a1 = _a1, b0 = _b0, b1 = _b1, c0 = _c0, c1 = _c1;

            for (var i = 0; i < blocks; i++)
            {
                var t1_z = ((a0 >> 27) | (a1 << 37)) ^ a0;
                var t2_z = ((b0 >> 15) | (b1 << 49)) ^ b0;
                var t3_z = ((c0 >> 45) | (c1 << 19)) ^ c0;

                var z = t1_z ^ t2_z ^ t3_z;

                res[i] = pt[i] ^ z;

                UpdateState64Scalar(ref a0, ref a1, ref b0, ref b1, ref c0, ref c1);
            }

            var tail = plaintext.Length & 7;
            if (tail > 0)
            {
                var t1_z = ((a0 >> 27) | (a1 << 37)) ^ a0;
                var t2_z = ((b0 >> 15) | (b1 << 49)) ^ b0;
                var t3_z = ((c0 >> 45) | (c1 << 19)) ^ c0;
                var z = t1_z ^ t2_z ^ t3_z;

                var bz = (byte*)&z;
                var pT = (byte*)pt + blocks * 8;
                var rT = (byte*)res + blocks * 8;

                for (var k = 0; k < tail; k++)
                {
                    rT[k] = (byte)(pT[k] ^ bz[k]);
                }

                UpdateState64Scalar(ref a0, ref a1, ref b0, ref b1, ref c0, ref c1);
            }

            _a0 = a0;
            _a1 = a1;
            _b0 = b0;
            _b1 = b1;
            _c0 = c0;
            _c1 = c1;
        }

        sw.Stop();
        LastGenerationTicks = sw.ElapsedTicks;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateState64Scalar(ref ulong a0, ref ulong a1, ref ulong b0, ref ulong b1, ref ulong c0, ref ulong c1)
    {
        var t1 = ((a0 >> 27) | (a1 << 37)) ^ a0;
        var t2 = ((b0 >> 15) | (b1 << 49)) ^ b0;
        var t3 = ((c0 >> 45) | (c1 << 19)) ^ c0;

        var r1 = t1 ^ (((a0 >> 2) | (a1 << 62)) & ((a0 >> 1) | (a1 << 63))) ^ ((b0 >> 6) | (b1 << 58));
        var r2 = t2 ^ (((b0 >> 2) | (b1 << 62)) & ((b0 >> 1) | (b1 << 63))) ^ ((c0 >> 24) | (c1 << 40));
        var r3 = t3 ^ (((c0 >> 2) | (c1 << 62)) & ((c0 >> 1) | (c1 << 63))) ^ ((a0 >> 24) | (a1 << 40));

        a0 = a1 | (r3 << 29);
        a1 = r3 >> 35;
        b0 = b1 | (r1 << 20);
        b1 = r1 >> 44;
        c0 = c1 | (r2 << 47);
        c1 = r2 >> 17;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void ProcessByteBatch8(int chunkIndex, byte* ptr)
    {
        Vector256<ulong> aA0 = Vector256.Create(_baseK0), aA1 = Vector256.Create(_baseK1);
        var aB0 = Avx2.Add(Vector256.Create(_baseIv0),
            Vector256.Create((ulong)chunkIndex, (ulong)chunkIndex + 1, (ulong)chunkIndex + 2, (ulong)chunkIndex + 3));
        var aB1 = Vector256.Create(_baseIv1);
        Vector256<ulong> aC0 = C0_Init, aC1 = Vector256<ulong>.Zero;

        Vector256<ulong> bA0 = Vector256.Create(_baseK0), bA1 = Vector256.Create(_baseK1);
        var bB0 = Avx2.Add(Vector256.Create(_baseIv0),
            Vector256.Create((ulong)chunkIndex + 4, (ulong)chunkIndex + 5, (ulong)chunkIndex + 6,
                (ulong)chunkIndex + 7));
        var bB1 = Vector256.Create(_baseIv1);
        Vector256<ulong> bC0 = C0_Init, bC1 = Vector256<ulong>.Zero;

        for (var w = 0; w < 18; w++)
        {
            UpdateStateV256_Inline(ref aA0, ref aA1, ref aB0, ref aB1, ref aC0, ref aC1);
            UpdateStateV256_Inline(ref bA0, ref bA1, ref bB0, ref bB1, ref bC0, ref bC1);
        }

        var baseOff = (long)chunkIndex * ChunkSize;
        var p0 = ptr + baseOff;
        var p1 = p0 + ChunkSize;
        var p2 = p1 + ChunkSize;
        var p3 = p2 + ChunkSize;
        var p4 = p3 + ChunkSize;
        var p5 = p4 + ChunkSize;
        var p6 = p5 + ChunkSize;
        var p7 = p6 + ChunkSize;

        for (var i = 0; i < ChunkSize; i += 128)
        {
            Sse.Prefetch0(p0 + i + 384);
            Sse.Prefetch0(p4 + i + 384);

            var z_a = CalculateZ_And_Update(ref aA0, ref aA1, ref aB0, ref aB1, ref aC0, ref aC1);
            var z_b = CalculateZ_And_Update(ref bA0, ref bA1, ref bB0, ref bB1, ref bC0, ref bC1);

            var z_a2 = CalculateZ_And_Update(ref aA0, ref aA1, ref aB0, ref aB1, ref aC0, ref aC1);
            var z_b2 = CalculateZ_And_Update(ref bA0, ref bA1, ref bB0, ref bB1, ref bC0, ref bC1);

            *(ulong*)(p0 + i) ^= z_a.GetElement(0);
            *(ulong*)(p0 + i + 8) ^= z_a2.GetElement(0);
            *(ulong*)(p1 + i) ^= z_a.GetElement(1);
            *(ulong*)(p1 + i + 8) ^= z_a2.GetElement(1);
            *(ulong*)(p2 + i) ^= z_a.GetElement(2);
            *(ulong*)(p2 + i + 8) ^= z_a2.GetElement(2);
            *(ulong*)(p3 + i) ^= z_a.GetElement(3);
            *(ulong*)(p3 + i + 8) ^= z_a2.GetElement(3);
            *(ulong*)(p4 + i) ^= z_b.GetElement(0);
            *(ulong*)(p4 + i + 8) ^= z_b2.GetElement(0);
            *(ulong*)(p5 + i) ^= z_b.GetElement(1);
            *(ulong*)(p5 + i + 8) ^= z_b2.GetElement(1);
            *(ulong*)(p6 + i) ^= z_b.GetElement(2);
            *(ulong*)(p6 + i + 8) ^= z_b2.GetElement(2);
            *(ulong*)(p7 + i) ^= z_b.GetElement(3);
            *(ulong*)(p7 + i + 8) ^= z_b2.GetElement(3);

            var z_a3 = CalculateZ_And_Update(ref aA0, ref aA1, ref aB0, ref aB1, ref aC0, ref aC1);
            var z_b3 = CalculateZ_And_Update(ref bA0, ref bA1, ref bB0, ref bB1, ref bC0, ref bC1);
            var z_a4 = CalculateZ_And_Update(ref aA0, ref aA1, ref aB0, ref aB1, ref aC0, ref aC1);
            var z_b4 = CalculateZ_And_Update(ref bA0, ref bA1, ref bB0, ref bB1, ref bC0, ref bC1);

            *(ulong*)(p0 + i + 16) ^= z_a3.GetElement(0);
            *(ulong*)(p0 + i + 24) ^= z_a4.GetElement(0);
            *(ulong*)(p1 + i + 16) ^= z_a3.GetElement(1);
            *(ulong*)(p1 + i + 24) ^= z_a4.GetElement(1);
            *(ulong*)(p2 + i + 16) ^= z_a3.GetElement(2);
            *(ulong*)(p2 + i + 24) ^= z_a4.GetElement(2);
            *(ulong*)(p3 + i + 16) ^= z_a3.GetElement(3);
            *(ulong*)(p3 + i + 24) ^= z_a4.GetElement(3);
            *(ulong*)(p4 + i + 16) ^= z_b3.GetElement(0);
            *(ulong*)(p4 + i + 24) ^= z_b4.GetElement(0);
            *(ulong*)(p5 + i + 16) ^= z_b3.GetElement(1);
            *(ulong*)(p5 + i + 24) ^= z_b4.GetElement(1);
            *(ulong*)(p6 + i + 16) ^= z_b3.GetElement(2);
            *(ulong*)(p6 + i + 24) ^= z_b4.GetElement(2);
            *(ulong*)(p7 + i + 16) ^= z_b3.GetElement(3);
            *(ulong*)(p7 + i + 24) ^= z_b4.GetElement(3);

            var z_a5 = CalculateZ_And_Update(ref aA0, ref aA1, ref aB0, ref aB1, ref aC0, ref aC1);
            var z_b5 = CalculateZ_And_Update(ref bA0, ref bA1, ref bB0, ref bB1, ref bC0, ref bC1);
            var z_a6 = CalculateZ_And_Update(ref aA0, ref aA1, ref aB0, ref aB1, ref aC0, ref aC1);
            var z_b6 = CalculateZ_And_Update(ref bA0, ref bA1, ref bB0, ref bB1, ref bC0, ref bC1);
            *(ulong*)(p0 + i + 32) ^= z_a5.GetElement(0);
            *(ulong*)(p0 + i + 40) ^= z_a6.GetElement(0);
            *(ulong*)(p1 + i + 32) ^= z_a5.GetElement(1);
            *(ulong*)(p1 + i + 40) ^= z_a6.GetElement(1);
            *(ulong*)(p2 + i + 32) ^= z_a5.GetElement(2);
            *(ulong*)(p2 + i + 40) ^= z_a6.GetElement(2);
            *(ulong*)(p3 + i + 32) ^= z_a5.GetElement(3);
            *(ulong*)(p3 + i + 40) ^= z_a6.GetElement(3);
            *(ulong*)(p4 + i + 32) ^= z_b5.GetElement(0);
            *(ulong*)(p4 + i + 40) ^= z_b6.GetElement(0);
            *(ulong*)(p5 + i + 32) ^= z_b5.GetElement(1);
            *(ulong*)(p5 + i + 40) ^= z_b6.GetElement(1);
            *(ulong*)(p6 + i + 32) ^= z_b5.GetElement(2);
            *(ulong*)(p6 + i + 40) ^= z_b6.GetElement(2);
            *(ulong*)(p7 + i + 32) ^= z_b5.GetElement(3);
            *(ulong*)(p7 + i + 40) ^= z_b6.GetElement(3);

            var z_a7 = CalculateZ_And_Update(ref aA0, ref aA1, ref aB0, ref aB1, ref aC0, ref aC1);
            var z_b7 = CalculateZ_And_Update(ref bA0, ref bA1, ref bB0, ref bB1, ref bC0, ref bC1);
            var z_a8 = CalculateZ_And_Update(ref aA0, ref aA1, ref aB0, ref aB1, ref aC0, ref aC1);
            var z_b8 = CalculateZ_And_Update(ref bA0, ref bA1, ref bB0, ref bB1, ref bC0, ref bC1);
            *(ulong*)(p0 + i + 48) ^= z_a7.GetElement(0);
            *(ulong*)(p0 + i + 56) ^= z_a8.GetElement(0);
            *(ulong*)(p1 + i + 48) ^= z_a7.GetElement(1);
            *(ulong*)(p1 + i + 56) ^= z_a8.GetElement(1);
            *(ulong*)(p2 + i + 48) ^= z_a7.GetElement(2);
            *(ulong*)(p2 + i + 56) ^= z_a8.GetElement(2);
            *(ulong*)(p3 + i + 48) ^= z_a7.GetElement(3);
            *(ulong*)(p3 + i + 56) ^= z_a8.GetElement(3);
            *(ulong*)(p4 + i + 48) ^= z_b7.GetElement(0);
            *(ulong*)(p4 + i + 56) ^= z_b8.GetElement(0);
            *(ulong*)(p5 + i + 48) ^= z_b7.GetElement(1);
            *(ulong*)(p5 + i + 56) ^= z_b8.GetElement(1);
            *(ulong*)(p6 + i + 48) ^= z_b7.GetElement(2);
            *(ulong*)(p6 + i + 56) ^= z_b8.GetElement(2);
            *(ulong*)(p7 + i + 48) ^= z_b7.GetElement(3);
            *(ulong*)(p7 + i + 56) ^= z_b8.GetElement(3);
        }
    }

    private void ProcessByteBatchSafe(int chunkIndex, int totalLen, byte* ptr)
    {
        for (var c = 0; c < 8; c++)
        {
            var idx = chunkIndex + c;
            var off = (long)idx * ChunkSize;
            if (off >= totalLen)
            {
                break;
            }

            var len = (int)System.Math.Min(ChunkSize, totalLen - off);
            var p = ptr + off;
            ulong a0 = _baseK0, a1 = _baseK1, b0 = _baseIv0 + (ulong)idx, b1 = _baseIv1, c0 = 7, c1 = 0;
            for (var i = 0; i < 18; i++)
            {
                UpdateState64Scalar(ref a0, ref a1, ref b0, ref b1, ref c0, ref c1);
            }

            for (var i = 0; i < len; i += 8)
            {
                UpdateState64Scalar(ref a0, ref a1, ref b0, ref b1, ref c0, ref c1);
                var z = ((a0 >> 27) | (a1 << 37)) ^ a0 ^ ((b0 >> 15) | (b1 << 49)) ^ b0 ^ ((c0 >> 45) | (c1 << 19)) ^
                        c0;
                if (i + 8 <= len)
                {
                    *(ulong*)(p + i) ^= z;
                }
                else
                {
                    var bz = (byte*)&z;
                    for (var j = 0; j < len - i; j++)
                    {
                        p[i + j] ^= bz[j];
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateStateV256_Inline(ref Vector256<ulong> a0, ref Vector256<ulong> a1, ref Vector256<ulong> b0,
        ref Vector256<ulong> b1, ref Vector256<ulong> c0, ref Vector256<ulong> c1)
    {
        var t1 = Avx2.Xor(Avx2.Or(Avx2.ShiftRightLogical(a0, 27), Avx2.ShiftLeftLogical(a1, 37)), a0);
        var t2 = Avx2.Xor(Avx2.Or(Avx2.ShiftRightLogical(b0, 15), Avx2.ShiftLeftLogical(b1, 49)), b0);
        var t3 = Avx2.Xor(Avx2.Or(Avx2.ShiftRightLogical(c0, 45), Avx2.ShiftLeftLogical(c1, 19)), c0);
        var r1 = Avx2.Xor(
            Avx2.Xor(t1,
                Avx2.And(Avx2.Or(Avx2.ShiftRightLogical(a0, 2), Avx2.ShiftLeftLogical(a1, 62)),
                    Avx2.Or(Avx2.ShiftRightLogical(a0, 1), Avx2.ShiftLeftLogical(a1, 63)))),
            Avx2.Or(Avx2.ShiftRightLogical(b0, 6), Avx2.ShiftLeftLogical(b1, 58)));
        var r2 = Avx2.Xor(
            Avx2.Xor(t2,
                Avx2.And(Avx2.Or(Avx2.ShiftRightLogical(b0, 2), Avx2.ShiftLeftLogical(b1, 62)),
                    Avx2.Or(Avx2.ShiftRightLogical(b0, 1), Avx2.ShiftLeftLogical(b1, 63)))),
            Avx2.Or(Avx2.ShiftRightLogical(c0, 24), Avx2.ShiftLeftLogical(c1, 40)));
        var r3 = Avx2.Xor(
            Avx2.Xor(t3,
                Avx2.And(Avx2.Or(Avx2.ShiftRightLogical(c0, 2), Avx2.ShiftLeftLogical(c1, 62)),
                    Avx2.Or(Avx2.ShiftRightLogical(c0, 1), Avx2.ShiftLeftLogical(c1, 63)))),
            Avx2.Or(Avx2.ShiftRightLogical(a0, 24), Avx2.ShiftLeftLogical(a1, 40)));
        a0 = Avx2.Or(a1, Avx2.ShiftLeftLogical(r3, 29));
        a1 = Avx2.ShiftRightLogical(r3, 35);
        b0 = Avx2.Or(b1, Avx2.ShiftLeftLogical(r1, 20));
        b1 = Avx2.ShiftRightLogical(r1, 44);
        c0 = Avx2.Or(c1, Avx2.ShiftLeftLogical(r2, 47));
        c1 = Avx2.ShiftRightLogical(r2, 17);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector256<ulong> CalculateZ_And_Update(ref Vector256<ulong> a0, ref Vector256<ulong> a1,
        ref Vector256<ulong> b0, ref Vector256<ulong> b1, ref Vector256<ulong> c0, ref Vector256<ulong> c1)
    {
        var t1 = Avx2.Xor(Avx2.Or(Avx2.ShiftRightLogical(a0, 27), Avx2.ShiftLeftLogical(a1, 37)), a0);
        var t2 = Avx2.Xor(Avx2.Or(Avx2.ShiftRightLogical(b0, 15), Avx2.ShiftLeftLogical(b1, 49)), b0);
        var t3 = Avx2.Xor(Avx2.Or(Avx2.ShiftRightLogical(c0, 45), Avx2.ShiftLeftLogical(c1, 19)), c0);
        var z = Avx2.Xor(Avx2.Xor(t1, t2), t3);
        var r1 = Avx2.Xor(
            Avx2.Xor(t1,
                Avx2.And(Avx2.Or(Avx2.ShiftRightLogical(a0, 2), Avx2.ShiftLeftLogical(a1, 62)),
                    Avx2.Or(Avx2.ShiftRightLogical(a0, 1), Avx2.ShiftLeftLogical(a1, 63)))),
            Avx2.Or(Avx2.ShiftRightLogical(b0, 6), Avx2.ShiftLeftLogical(b1, 58)));
        var r2 = Avx2.Xor(
            Avx2.Xor(t2,
                Avx2.And(Avx2.Or(Avx2.ShiftRightLogical(b0, 2), Avx2.ShiftLeftLogical(b1, 62)),
                    Avx2.Or(Avx2.ShiftRightLogical(b0, 1), Avx2.ShiftLeftLogical(b1, 63)))),
            Avx2.Or(Avx2.ShiftRightLogical(c0, 24), Avx2.ShiftLeftLogical(c1, 40)));
        var r3 = Avx2.Xor(
            Avx2.Xor(t3,
                Avx2.And(Avx2.Or(Avx2.ShiftRightLogical(c0, 2), Avx2.ShiftLeftLogical(c1, 62)),
                    Avx2.Or(Avx2.ShiftRightLogical(c0, 1), Avx2.ShiftLeftLogical(c1, 63)))),
            Avx2.Or(Avx2.ShiftRightLogical(a0, 24), Avx2.ShiftLeftLogical(a1, 40)));
        a0 = Avx2.Or(a1, Avx2.ShiftLeftLogical(r3, 29));
        a1 = Avx2.ShiftRightLogical(r3, 35);
        b0 = Avx2.Or(b1, Avx2.ShiftLeftLogical(r1, 20));
        b1 = Avx2.ShiftRightLogical(r1, 44);
        c0 = Avx2.Or(c1, Avx2.ShiftLeftLogical(r2, 47));
        c1 = Avx2.ShiftRightLogical(r2, 17);
        return z;
    }
}