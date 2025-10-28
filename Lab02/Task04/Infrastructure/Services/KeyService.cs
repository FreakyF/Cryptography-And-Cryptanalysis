using Task04.Application.Abstractions;

namespace Task04.Infrastructure.Services;

public sealed class KeyService(IFileService fileService) : IKeyService
{
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

    private static bool TryParseInvariantInt(ReadOnlySpan<char> s, out int value)
    {
        return int.TryParse(
            s,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    private static bool IsInvertibleMod26(int a)
    {
        a = Mod(a, 26);
        return Gcd(a, 26) == 1;
    }

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

    private static int Mod(int v, int m)
    {
        var r = v % m;
        return r < 0 ? r + m : r;
    }
}