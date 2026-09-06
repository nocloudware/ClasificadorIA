using System.IO;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

public static class FileOrganizer
{
    public static string SaneateFolderName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "SinCategoria";
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Length > 50 ? name[..50] : name.Trim();
    }

    public static OrganizeResult Organize(
        IReadOnlyDictionary<string, string> filePaths, string destDir, IReadOnlyList<ClassificationResult> results,
        bool copy, CancellationToken ct, Action<int, int>? onProgress = null,
        Action<string, bool>? onFile = null)
    {
        Directory.CreateDirectory(destDir);

        int total = results.Sum(r => r.Files.Count);
        int processed = 0;
        int errors = 0;

        foreach (var result in results)
        {
            if (ct.IsCancellationRequested) break;

            string folder = SaneateFolderName(result.Category);
            string dir = Path.Combine(destDir, folder);
            try { Directory.CreateDirectory(dir); }
            catch { continue; }

            foreach (string file in result.Files)
            {
                if (ct.IsCancellationRequested) break;
                bool ok = false;
                try
                {
                    string src = filePaths.TryGetValue(file, out var full) ? full : file;
                    string dest = Path.Combine(dir, file);
                    if (File.Exists(src))
                    {
                        if (copy) File.Copy(src, dest, true);
                        else File.Move(src, dest, true);
                        processed++;
                        ok = true;
                    }
                    else
                    {
                        errors++;
                    }
                }
                catch
                {
                    errors++;
                }
                onFile?.Invoke(file, ok);
                onProgress?.Invoke(processed, errors);
            }
        }

        return new OrganizeResult(processed, errors);
    }
}