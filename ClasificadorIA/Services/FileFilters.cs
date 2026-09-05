namespace ClasificadorIA.Services;

public static class FileFilters
{
    private static readonly string[] SystemExtensions = { ".exe", ".dll", ".ps1", ".bat", ".cs", ".csproj" };

    public static bool IsSystemFile(string fileName) =>
        SystemExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
}