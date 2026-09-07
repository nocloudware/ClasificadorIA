using System.IO;
using System.Text.RegularExpressions;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

/// <summary>Clasificación local sin LLM: tema por tokens comunes del nombre + fallback por extensión.</summary>
public static class LocalClassifier
{
    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        // ES
        "el", "la", "los", "las", "de", "del", "al", "a", "y", "e", "o", "u", "en", "con", "por", "para", "un", "una", "unos", "unas", "que", "como",
        // EN
        "the", "of", "and", "to", "in", "on", "for", "with", "is", "are", "was", "were", "a", "an"
    };

    private static readonly HashSet<string> ExtAudio = new(StringComparer.OrdinalIgnoreCase) { "mp3", "wav", "flac", "ogg", "m4a", "aac", "opus", "wma" };
    private static readonly HashSet<string> ExtVideo = new(StringComparer.OrdinalIgnoreCase) { "mp4", "mkv", "avi", "mov", "wmv", "flv", "webm", "m4v" };
    private static readonly HashSet<string> ExtImage = new(StringComparer.OrdinalIgnoreCase) { "jpg", "jpeg", "png", "gif", "bmp", "webp", "svg", "ico", "heic", "tiff" };
    private static readonly HashSet<string> ExtDoc = new(StringComparer.OrdinalIgnoreCase) { "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "txt", "md", "csv", "rtf", "odt", "ods" };

    public static List<ClassificationResult> Classify(IReadOnlyList<string> filenames, int depth, Idioma idioma)
    {
        if (filenames.Count == 0) return new List<ClassificationResult>();

        var tokensByFile = new Dictionary<string, List<string>>(filenames.Count, StringComparer.OrdinalIgnoreCase);
        var tokenFreq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in filenames)
        {
            var tokens = Tokenize(file);
            tokensByFile[file] = tokens;
            foreach (var t in tokens.Distinct(StringComparer.OrdinalIgnoreCase))
                tokenFreq[t] = tokenFreq.TryGetValue(t, out int c) ? c + 1 : 1;
        }

        // Categoría por tema: token compartido más frecuente que contiene el archivo.
        var topicByFile = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (file, tokens) in tokensByFile)
        {
            string? best = null;
            int bestFreq = 0;
            foreach (var t in tokens)
            {
                if (!tokenFreq.TryGetValue(t, out int freq) || freq < 2 || freq <= bestFreq) continue;
                best = t;
                bestFreq = freq;
            }
            if (best != null)
                topicByFile[file] = Capitalize(best);
        }

        // Buckets por tema; el resto por extensión.
        var buckets = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [Translations.Get("Otros", idioma)] = new List<string>()
        };
        foreach (var file in filenames)
        {
            string cat = topicByFile.TryGetValue(file, out string? topic)
                ? topic
                : ExtensionCategory(Path.GetExtension(file), idioma);
            if (!buckets.TryGetValue(cat, out var list))
                buckets[cat] = list = new List<string>();
            list.Add(file);
        }

        var results = buckets
            .Where(kv => kv.Value.Count > 0)
            .Select(kv => new ClassificationResult(kv.Key, kv.Value.AsReadOnly()))
            .ToList();
        return CapToDepth(results, depth, Translations.Get("Otros", idioma));
    }

    private static List<string> Tokenize(string filename)
    {
        string name = Path.GetFileNameWithoutExtension(filename);
        return Regex.Split(name, @"[^a-zA-Z0-9áéíóúüñÁÉÍÓÚÜÑ]+")
            .Where(t => t.Length >= 3 && !Stopwords.Contains(t))
            .Select(t => t.ToLowerInvariant())
            .ToList();
    }

    private static string Capitalize(string token) =>
        token.Length == 0 ? token : char.ToUpperInvariant(token[0]) + token[1..];

    private static string ExtensionCategory(string ext, Idioma idioma)
    {
        string baseExt = ext.TrimStart('.');
        if (ExtAudio.Contains(baseExt)) return Translations.Get("CatAudio", idioma);
        if (ExtVideo.Contains(baseExt)) return Translations.Get("CatVideo", idioma);
        if (ExtImage.Contains(baseExt)) return Translations.Get("CatImage", idioma);
        if (ExtDoc.Contains(baseExt)) return Translations.Get("CatDoc", idioma);
        return Translations.Get("Otros", idioma);
    }

    // Máximo `depth` categorías: conserva las depth-1 mayores y fusiona el resto en `others`.
    public static List<ClassificationResult> CapToDepth(IReadOnlyList<ClassificationResult> results, int depth, string others)
    {
        if (results.Count <= depth) return results.ToList();
        var ordered = results.OrderByDescending(r => r.Files.Count).Take(depth - 1).ToList();
        var leftover = results
            .Where(r => !ordered.Contains(r))
            .SelectMany(r => r.Files)
            .ToList();
        if (leftover.Count > 0)
            ordered.Add(new ClassificationResult(others, leftover.AsReadOnly()));
        return ordered;
    }
}