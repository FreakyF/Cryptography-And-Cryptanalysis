namespace Task03.Application.Abstractions;

public interface INGramCounter
{
    IReadOnlyDictionary<string, int> Count(string normalized, int n);
}