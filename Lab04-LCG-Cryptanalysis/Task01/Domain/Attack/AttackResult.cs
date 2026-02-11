using System.Numerics;

namespace Task01.Domain.Attack;

public sealed class AttackResult
{
    private AttackResult(bool success, BigInteger a, BigInteger b, BigInteger? gcdMuModulus)
    {
        Success = success;
        A = a;
        B = b;
        GcdMuModulus = gcdMuModulus;
        FailureReason = string.Empty;
        IsInsufficientKeystream = false;
        IsAmbiguousSolutions = false;
        IsVerificationFailed = false;
        RequiredKeystreamBits = null;
        ActualKeystreamBits = null;
    }

    private AttackResult(
        string failureReason,
        bool isInsufficientKeystream,
        bool isAmbiguousSolutions,
        bool isVerificationFailed,
        BigInteger? gcdMuModulus,
        int? requiredKeystreamBits,
        int? actualKeystreamBits)
    {
        Success = false;
        A = BigInteger.Zero;
        B = BigInteger.Zero;
        FailureReason = failureReason;
        IsInsufficientKeystream = isInsufficientKeystream;
        IsAmbiguousSolutions = isAmbiguousSolutions;
        IsVerificationFailed = isVerificationFailed;
        GcdMuModulus = gcdMuModulus;
        RequiredKeystreamBits = requiredKeystreamBits;
        ActualKeystreamBits = actualKeystreamBits;
    }

    public bool Success { get; }
    public BigInteger A { get; }
    public BigInteger B { get; }
    public string FailureReason { get; }
    public bool IsInsufficientKeystream { get; }
    public bool IsAmbiguousSolutions { get; }
    public bool IsVerificationFailed { get; }
    public BigInteger? GcdMuModulus { get; }
    public int? RequiredKeystreamBits { get; }
    public int? ActualKeystreamBits { get; }

    public static AttackResult Succeeded(BigInteger a, BigInteger b, BigInteger? gcdMuModulus)
    {
        return new AttackResult(true, a, b, gcdMuModulus);
    }

    public static AttackResult Failed(string reason)
    {
        return new AttackResult(
            reason,
            false,
            false,
            false,
            null,
            null,
            null);
    }

    public static AttackResult InsufficientKeystream(int requiredKeystreamBits, int actualKeystreamBits)
    {
        var message =
            $"Known keystream is too short: required at least {requiredKeystreamBits} bits, got {actualKeystreamBits}.";
        return new AttackResult(
            message,
            true,
            false,
            false,
            null,
            requiredKeystreamBits,
            actualKeystreamBits);
    }

    public static AttackResult AmbiguousSolutions(BigInteger gcdMuModulus)
    {
        var message =
            $"Ambiguous solutions: gcd(mu, m) = {gcdMuModulus} ≠ 1. Additional state information is required.";
        return new AttackResult(
            message,
            false,
            true,
            false,
            gcdMuModulus,
            null,
            null);
    }

    public static AttackResult VerificationFailed()
    {
        const string message = "Recovered parameters do not reproduce the third state S3.";
        return new AttackResult(
            message,
            false,
            false,
            true,
            null,
            null,
            null);
    }
}
