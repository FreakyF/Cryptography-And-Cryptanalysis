using Task03.Application.Abstractions;

namespace Task03.Infrastructure.Services;

public sealed class FileService : IFileService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    /// <summary>Asynchronously reads the entire contents of the file at the supplied path.</summary>
    /// <param name="path">The path of the file to read.</param>
    /// <returns>The complete text stored in the target file.</returns>
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

    /// <summary>Asynchronously writes the provided content to the file at the supplied path.</summary>
    /// <param name="path">The destination file path to create or overwrite.</param>
    /// <param name="content">The textual content that should be persisted.</param>
    /// <returns>A task that completes when the data has been written and flushed.</returns>
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