using Task03.Application.Abstractions;

namespace Task03.Infrastructure.Services;

public sealed class KeyService(IFileService fileService) : IKeyService
{
    /// <summary>Loads, parses, and validates the affine key pair from the specified key file.</summary>
    /// <param name="keyFilePath">The path to the file containing the two integer key components.</param>
    /// <returns>The tuple of multiplicative and additive key values.</returns>
    /// <exception cref="FormatException">Thrown when the key file is missing, malformed, or not invertible.</exception>
    public async Task<(int A, int B)> GetKeyAsync(string keyFilePath)
    {
        var raw = await fileService.ReadAllTextAsync(keyFilePath).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new FormatException("Key file is empty");
        }

        var span = raw.AsSpan();

        var firstLineEnd = span.IndexOfAny('\r', '\n');
        if (firstLineEnd >= 0)
        {
            span = span[..firstLineEnd];
        }

        span = TrimWhite(span);

        if (span.IsEmpty)
        {
            throw new FormatException("Key not found");
        }

        SplitTwo(span, out var firstPart, out var secondPart);

        if (!TryParseInvariantInt(firstPart, out var a) ||
            !TryParseInvariantInt(secondPart, out var b))
        {
            throw new FormatException("Key is not valid");
        }

        return !IsInvertibleMod26(a) ? throw new FormatException("Key 'a' is not invertible modulo 26") : (a, b);
    }

    /// <summary>Removes leading and trailing whitespace characters from the provided span.</summary>
    /// <param name="value">The span of characters to trim.</param>
    /// <returns>The span segment without surrounding whitespace.</returns>
    private static ReadOnlySpan<char> TrimWhite(ReadOnlySpan<char> value)
    {
        var start = 0;
        var end = value.Length - 1;

        while (start <= end && char.IsWhiteSpace(value[start]))
        {
            start++;
        }

        while (end >= start && char.IsWhiteSpace(value[end]))
        {
            end--;
        }

        return start > end
            ? ReadOnlySpan<char>.Empty
            : value.Slice(start, end - start + 1);
    }

    /// <summary>Splits the provided span into two whitespace-separated parts.</summary>
    /// <param name="span">The span containing two integer tokens.</param>
    /// <param name="first">When the method returns, contains the trimmed first token.</param>
    /// <param name="second">When the method returns, contains the trimmed second token.</param>
    /// <exception cref="FormatException">Thrown when two integers cannot be located.</exception>
    private static void SplitTwo(ReadOnlySpan<char> span, out ReadOnlySpan<char> first, out ReadOnlySpan<char> second)
    {
        var sep = span.IndexOfAny(' ', '\t');
        if (sep < 0)
        {
            throw new FormatException("Key must contain two integers");
        }

        first = span[..sep];

        var restStart = sep + 1;
        while (restStart < span.Length && char.IsWhiteSpace(span[restStart]))
        {
            restStart++;
        }

        if (restStart >= span.Length)
        {
            throw new FormatException("Key must contain two integers");
        }

        second = span[restStart..];

        first = TrimWhite(first);
        second = TrimWhite(second);
    }

    /// <summary>Attempts to parse an invariant-culture integer from the provided span.</summary>
    /// <param name="s">The characters representing the integer to parse.</param>
    /// <param name="value">When the method returns, contains the parsed integer if successful.</param>
    /// <returns><c>true</c> if the span was parsed successfully; otherwise, <c>false</c>.</returns>
    private static bool TryParseInvariantInt(ReadOnlySpan<char> s, out int value)
    {
        return int.TryParse(
            s,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    /// <summary>Checks whether the provided integer has a multiplicative inverse modulo 26.</summary>
    /// <param name="a">The integer to test for invertibility.</param>
    /// <returns><c>true</c> when the value is coprime with 26; otherwise, <c>false</c>.</returns>
    private static bool IsInvertibleMod26(int a)
    {
        a = Mod(a, 26);
        return Gcd(a, 26) == 1;
    }

    /// <summary>Computes the greatest common divisor of the two supplied integers.</summary>
    /// <param name="x">The first integer operand.</param>
    /// <param name="y">The second integer operand.</param>
    /// <returns>The non-negative greatest common divisor of the operands.</returns>
    private static int Gcd(int x, int y)
    {
        while (y != 0)
        {
            var t = x % y;
            x = y;
            y = t;
        }

        return x < 0 ? -x : x;
    }

    /// <summary>Produces the non-negative remainder of the given value modulo the specified modulus.</summary>
    /// <param name="v">The integer value to reduce.</param>
    /// <param name="m">The modulus that defines the arithmetic space.</param>
    /// <returns>The remainder in the range [0, m).</returns>
    private static int Mod(int v, int m)
    {
        var r = v % m;
        return r < 0 ? r + m : r;
    }
}