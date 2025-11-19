namespace Task04.Infrastructure.Services;

public static class TempFiles
{
    public static string EnsureDir(string baseDir, string name)
    {
        var path = Path.Combine(baseDir, name);
        Directory.CreateDirectory(path);
        return path;
    }
}