using System.Text.Json;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

public static class ResponseParser
{
    private static string Normalize(string name) => name.Trim().ToUpperInvariant();

    public static List<ClassificationResult> Parse(string response, IEnumerable<string> realFiles, Idioma idioma)
    {
        if (string.IsNullOrWhiteSpace(response))
            return new List<ClassificationResult>();

        var result = new List<ClassificationResult>();
        using var doc = ParseJson(response);
        if (doc == null) return result;

        var root = doc.RootElement;
        var categorias = FindCategorias(root, idioma);
        if (categorias is not JsonElement cats || cats.ValueKind != JsonValueKind.Object)
            return result;

        var realByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var real in realFiles)
        {
            string key = Normalize(real);
            realByName.TryAdd(key, real);
        }

        foreach (var prop in cats.EnumerateObject())
        {
            string category = FileOrganizer.SaneateFolderName(prop.Name);
            if (string.IsNullOrEmpty(category)) continue;

            var matched = new List<string>();
            foreach (var nameToken in prop.Value.EnumerateArray())
            {
                string? name = nameToken.ValueKind == JsonValueKind.String ? nameToken.GetString() : null;
                if (string.IsNullOrEmpty(name)) continue;
                if (realByName.TryGetValue(Normalize(name), out string? real))
                    matched.Add(real);
            }

            if (matched.Count > 0)
                result.Add(new ClassificationResult(category, matched));
        }

        return result;
    }

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

    private static JsonElement? FindCategorias(JsonElement root, Idioma idioma)
    {
        string primary = idioma == Idioma.Español ? "categorias" : "categories";
        string fallback = idioma == Idioma.Español ? "categories" : "categorias";
        foreach (var key in new[] { primary, fallback })
            if (root.TryGetProperty(key, out var cats) && cats.ValueKind == JsonValueKind.Object)
                return cats;
        return null;
    }
}