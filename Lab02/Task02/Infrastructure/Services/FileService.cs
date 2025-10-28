using Task02.Application.Abstractions;

namespace Task02.Infrastructure.Services;

public sealed class FileService : IFileService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    /// <summary>Reads the entire contents of a file asynchronously into a string.</summary>
    /// <param name="path">The path of the file to read.</param>
    /// <returns>A task that yields the full text contained in the file.</returns>
    public async Task<string> ReadAllTextAsync(string path)
    {
        const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

        await using var fs = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            fileOptions
        );

        using var reader = new StreamReader(fs, Encoding.UTF8, true);

        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    /// <summary>Writes the provided text to a file asynchronously, replacing any existing content.</summary>
    /// <param name="path">The path of the file to write.</param>
    /// <param name="content">The text content that should be stored in the file.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    public async Task WriteAllTextAsync(string path, string content)
    {
        const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

        await using var fs = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            4096,
            fileOptions
        );

        await using var writer = new StreamWriter(fs, Utf8NoBom);

        await writer.WriteAsync(content.AsMemory()).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
    }
}