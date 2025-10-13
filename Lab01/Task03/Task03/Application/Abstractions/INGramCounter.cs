namespace Task02.Application.Abstractions;

public interface INGramCounter
{
    IReadOnlyDictionary<string, int> Count(string normalized, int n);
}