using System.Collections.Generic;
using System.Text.Json;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

public static class ResponseParser
{
    // Claves JSON que puede devolver el modelo según el idioma del prompt (8 idiomas).
    private static readonly string[] FileArrayKeys =
        { "archivos", "files", "fichiers", "dateien", "arquivos", "file", "ファイル", "文件" };
    private static readonly string[] ItemKeys =
        { "archivo", "file", "fichier", "datei", "arquivo", "ファイル", "文件" };
    private static readonly string[] CategoryKeys =
        { "categoria", "category", "categorie", "kategorie", "カテゴリ", "分类" };
    private static readonly string[] FinalMapKeys =
        { "finales", "final", "finaux", "finais", "finali", "最終", "最终" };

    private static string Normalize(string name) => name.Trim().ToUpperInvariant();

    private static JsonDocument? ParseJson(string response)
    {
        int start = response.IndexOf('{');
        int end = response.LastIndexOf('}') + 1;
        if (start < 0 || end <= start) return null;
        try
        {
            return JsonDocument.Parse(response.Substring(start, end - start));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // {"archivos":[{archivo,categoria}]} y variantes en otros idiomas.
    public static Dictionary<string, string> ParseBatchAssignments(string response, IEnumerable<string> realFiles, Idioma idioma)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(response)) return result;

        using var doc = ParseJson(response);
        if (doc == null) return result;
        var root = doc.RootElement;

        var realByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var real in realFiles)
            realByName.TryAdd(Normalize(real), real);

        JsonElement? arr = null;
        foreach (var key in FileArrayKeys)
            if (root.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.Array) { arr = el; break; }
        if (arr == null) return result;

        foreach (var entry in arr.Value.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object) continue;
            string? name = null, cat = null;
            foreach (var key in ItemKeys)
                if (entry.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.String) { name = el.GetString(); break; }
            foreach (var key in CategoryKeys)
                if (entry.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.String) { cat = el.GetString(); break; }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(cat)) continue;
            cat = FileOrganizer.SaneateFolderName(cat);
            if (string.IsNullOrEmpty(cat)) continue;
            if (realByName.TryGetValue(Normalize(name), out string? real))
                result.TryAdd(real, cat);
        }
        return result;
    }

    // {"finales":{"Final":["cat1","cat2"]}} y variantes en otros idiomas.
    public static Dictionary<string, string[]>? ParseConsolidationMap(string response, Idioma idioma)
    {
        if (string.IsNullOrWhiteSpace(response)) return null;
        using var doc = ParseJson(response);
        if (doc == null) return null;
        var root = doc.RootElement;

        JsonElement? map = null;
        foreach (var key in FinalMapKeys)
            if (root.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.Object) { map = el; break; }
        if (map == null) return null;

        var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in map.Value.EnumerateObject())
        {
            string cat = FileOrganizer.SaneateFolderName(prop.Name);
            if (string.IsNullOrEmpty(cat)) continue;
            var sources = new List<string>();
            if (prop.Value.ValueKind == JsonValueKind.Array)
                foreach (var s in prop.Value.EnumerateArray())
                    if (s.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(s.GetString()))
                        sources.Add(s.GetString()!);
            if (sources.Count > 0)
                result[cat] = sources.ToArray();
        }
        return result.Count > 0 ? result : null;
    }
}