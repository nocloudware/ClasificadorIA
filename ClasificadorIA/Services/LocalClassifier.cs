using System.IO;
using System.Text.RegularExpressions;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

/// <summary>Clasificación local sin LLM. Clasifica SOLO por el criterio elegido:
/// metadatos para el criterio, o tokens compartidos para "Tema". Lo que no calza va a "Otros".</summary>
public static class LocalClassifier
{
    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        // ES
        "el", "la", "los", "las", "de", "del", "al", "a", "y", "e", "o", "u", "en", "con", "por", "para", "un", "una", "unos", "unas", "que", "qué", "como", "cómo", "más",
        // EN
        "the", "of", "and", "to", "in", "on", "for", "with", "is", "are", "was", "were", "a", "an"
    };

    private static readonly HashSet<string> ExtAudio = new(StringComparer.OrdinalIgnoreCase) { "mp3", "wav", "flac", "ogg", "m4a", "aac", "opus", "wma" };
    private static readonly HashSet<string> ExtVideo = new(StringComparer.OrdinalIgnoreCase) { "mp4", "mkv", "avi", "mov", "wmv", "flv", "webm", "m4v" };
    private static readonly HashSet<string> ExtImage = new(StringComparer.OrdinalIgnoreCase) { "jpg", "jpeg", "png", "gif", "bmp", "webp", "svg", "ico", "heic", "tiff" };
    private static readonly HashSet<string> ExtDoc = new(StringComparer.OrdinalIgnoreCase) { "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "txt", "md", "csv", "rtf", "odt", "ods" };

    internal static bool IsImageExt(string ext) => ExtImage.Contains(ext.TrimStart('.'));
    internal static bool IsAudioExt(string ext) => ExtAudio.Contains(ext.TrimStart('.'));
    internal static bool IsVideoExt(string ext) => ExtVideo.Contains(ext.TrimStart('.'));
    internal static bool IsDocExt(string ext) => ExtDoc.Contains(ext.TrimStart('.'));

    public static List<ClassificationResult> Classify(
        IReadOnlyList<string> paths, int depth, Idioma idioma, string? criterionKey = null)
    {
        if (paths.Count == 0) return new List<ClassificationResult>();

        string others = Translations.Get("Otros", idioma);
        bool byTokens = string.IsNullOrWhiteSpace(criterionKey) ||
                        string.Equals(criterionKey, "Tema", StringComparison.OrdinalIgnoreCase);

        var buckets = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var remaining = new List<string>();
        foreach (var p in paths)
        {
            string? cat = MetadataClassifier.TryClassify(p, criterionKey, idioma);
            if (cat != null) AddTo(buckets, cat, p);
            else remaining.Add(p);
        }

        // Sin metadatos para el criterio: "Tema" agrupa por tokens compartidos; el resto va a Otros.
        if (byTokens && remaining.Count > 0)
        {
            var topicByFile = ClassifyByTokens(remaining);
            foreach (var f in remaining)
                AddTo(buckets, topicByFile.TryGetValue(f, out string? topic) ? topic : others, f);
        }
        else
        {
            foreach (var f in remaining)
                AddTo(buckets, others, f);
        }

        var results = buckets
            .Where(kv => kv.Value.Count > 0)
            .Select(kv => new ClassificationResult(kv.Key, kv.Value.AsReadOnly()))
            .ToList();
        return CapToDepth(results, depth, others);
    }

    private static void AddTo(Dictionary<string, List<string>> buckets, string cat, string file)
    {
        if (!buckets.TryGetValue(cat, out var list))
            buckets[cat] = list = new List<string>();
        list.Add(file);
    }

    /// <summary>Categoría por tema: token compartido más frecuente en el nombre.
    /// Un token en casi todos los archivos es el tema de la carpeta (p. ej. un sufijo de serie),
    /// no un subtema: se descarta para no colapsar todo en una sola categoría.</summary>
    private static Dictionary<string, string> ClassifyByTokens(IReadOnlyList<string> filenames)
    {
        var topicByFile = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (filenames.Count == 0) return topicByFile;

        var tokensByFile = new Dictionary<string, List<string>>(filenames.Count, StringComparer.OrdinalIgnoreCase);
        var tokenFreq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in filenames)
        {
            var tokens = Tokenize(file);
            tokensByFile[file] = tokens;
            foreach (var t in tokens.Distinct(StringComparer.OrdinalIgnoreCase))
                tokenFreq[t] = tokenFreq.TryGetValue(t, out int c) ? c + 1 : 1;
        }

        double coverLimit = filenames.Count * 0.75;
        foreach (var (file, tokens) in tokensByFile)
        {
            string? best = null;
            int bestFreq = 0;
            foreach (var t in tokens)
            {
                if (!tokenFreq.TryGetValue(t, out int freq) || freq < 2 || freq > coverLimit || freq <= bestFreq) continue;
                best = t;
                bestFreq = freq;
            }
            if (best != null)
                topicByFile[file] = Capitalize(best);
        }

        return topicByFile;
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

    // Máximo `depth` categorías: conserva las depth-1 mayores y fusiona el resto en `others`.
    // Si ya existe una categoría `others`, el sobrante se suma a ella (no se duplica).
    public static List<ClassificationResult> CapToDepth(IReadOnlyList<ClassificationResult> results, int depth, string others)
    {
        if (results.Count <= depth) return results.ToList();
        var ordered = results.OrderByDescending(r => r.Files.Count).Take(depth - 1).ToList();
        var leftover = results
            .Where(r => !ordered.Contains(r))
            .SelectMany(r => r.Files)
            .ToList();
        if (leftover.Count > 0)
        {
            var existing = ordered.FirstOrDefault(r => string.Equals(r.Category, others, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                int idx = ordered.IndexOf(existing);
                ordered[idx] = new ClassificationResult(existing.Category, existing.Files.Concat(leftover).ToList().AsReadOnly());
            }
            else
            {
                ordered.Add(new ClassificationResult(others, leftover.AsReadOnly()));
            }
        }
        return ordered;
    }
}