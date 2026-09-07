using System.Text.Json;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

public static class ResponseParser
{
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

    // {"archivos":[{archivo,categoria}]} / EN {"files":[{file,category}]}
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

        string primary = idioma == Idioma.Español ? "archivos" : "files";
        string fallback = idioma == Idioma.Español ? "files" : "archivos";
        JsonElement? arr = null;
        foreach (var key in new[] { primary, fallback })
            if (root.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.Array) { arr = el; break; }
        if (arr == null) return result;

        string fileKey = idioma == Idioma.Español ? "archivo" : "file";
        string catKey = idioma == Idioma.Español ? "categoria" : "category";
        string fileKeyFallback = idioma == Idioma.Español ? "file" : "archivo";
        string catKeyFallback = idioma == Idioma.Español ? "category" : "categoria";

        foreach (var entry in arr.Value.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object) continue;
            string? name = null, cat = null;
            foreach (var key in new[] { fileKey, fileKeyFallback })
                if (entry.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.String) { name = el.GetString(); break; }
            foreach (var key in new[] { catKey, catKeyFallback })
                if (entry.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.String) { cat = el.GetString(); break; }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(cat)) continue;
            cat = FileOrganizer.SaneateFolderName(cat);
            if (string.IsNullOrEmpty(cat)) continue;
            if (realByName.TryGetValue(Normalize(name), out string? real))
                result.TryAdd(real, cat);
        }
        return result;
    }

    // {"finales":{"Final":["cat1","cat2"]}} / EN {"final":{...}}
    public static Dictionary<string, string[]>? ParseConsolidationMap(string response, Idioma idioma)
    {
        if (string.IsNullOrWhiteSpace(response)) return null;
        using var doc = ParseJson(response);
        if (doc == null) return null;
        var root = doc.RootElement;

        string primary = idioma == Idioma.Español ? "finales" : "final";
        string fallback = idioma == Idioma.Español ? "final" : "finales";
        JsonElement? map = null;
        foreach (var key in new[] { primary, fallback })
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