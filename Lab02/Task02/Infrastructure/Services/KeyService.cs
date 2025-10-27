using Task02.Application.Abstractions;

namespace Task02.Infrastructure.Services;

public sealed class KeyService(IFileService fileService) : IKeyService
{
    public async Task<int> GetKeyAsync(string keyFilePath)
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

        return TryParseInvariantInt(span, out var value)
            ? value
            : throw new FormatException("Key is not a valid integer");
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

        return start > end ? ReadOnlySpan<char>.Empty : value.Slice(start, end - start + 1);
    }

    private static bool TryParseInvariantInt(ReadOnlySpan<char> span, out int result)
    {
        return int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }
}