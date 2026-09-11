using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

/// <summary>Extrae la categoría de un archivo SOLO según el criterio elegido.
/// Cada archivo aporta metadatos solo si ese criterio aplica a su tipo; si no, devuelve null.</summary>
public static class MetadataClassifier
{
    private static readonly Dictionary<string, string> GenreMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["other"] = "Otros",
        ["others"] = "Otros",
        ["misc"] = "Otros",
        ["miscellaneous"] = "Otros",
        ["unknown"] = "Otros",
        ["rhythm and blues"] = "R&B",
        ["rnb"] = "R&B",
        ["hip hop"] = "Hip Hop",
        ["hip-hop"] = "Hip Hop",
        ["rap"] = "Hip Hop",
        ["synthpop"] = "New Wave",
        ["synth pop"] = "New Wave",
        ["synth-pop"] = "New Wave",
        ["new romantic"] = "New Wave",
        ["latin pop"] = "Latin",
        ["latino"] = "Latin",
        ["films"] = "Banda sonora",
        ["film score"] = "Banda sonora",
        ["soundtrack"] = "Banda sonora",
        ["movie soundtrack"] = "Banda sonora"
    };

    public static string[] GetCells(string path, ClassificationMode mode, Idioma idioma)
    {
        var cells = new string[mode.Criterios.Length];
        for (int i = 0; i < cells.Length; i++)
            cells[i] = TryClassify(path, mode.Criterios[i], idioma) ?? "";
        return cells;
    }

    public static Dictionary<string, string?> GetAllMetadata(string path, Idioma idioma)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        string[] allCriteria = { "Tema", "Género", "Artista", "Álbum", "Época", "Año", "Director", "Saga", "Cadena", "Autor", "Editorial" };
        foreach (var c in allCriteria)
            result[c] = TryClassify(path, c, idioma);
        return result;
    }

    public static string? TryClassify(string path, string? criterionKey, Idioma idioma)
    {
        string key = criterionKey ?? "Tema";
        if (string.Equals(key, "Tema", StringComparison.OrdinalIgnoreCase))
            return null; // "Tema" se resuelve por tokens en LocalClassifier.

        string ext = Path.GetExtension(path);
        bool audio = LocalClassifier.IsAudioExt(ext);
        bool video = LocalClassifier.IsVideoExt(ext);
        bool image = LocalClassifier.IsImageExt(ext);
        bool doc = LocalClassifier.IsDocExt(ext);

        if (string.Equals(key, "Género", StringComparison.OrdinalIgnoreCase))
            return audio ? AudioTag(path, f => NormalizeGenre(First(f.Tag.Genres), idioma)) : null;
        if (string.Equals(key, "Artista", StringComparison.OrdinalIgnoreCase))
            return audio ? AudioTag(path, f => Clean(First(f.Tag.Performers))) : null;
        if (string.Equals(key, "Álbum", StringComparison.OrdinalIgnoreCase))
            return audio ? AudioTag(path, f => Clean(f.Tag.Album)) : null;
        if (string.Equals(key, "Época", StringComparison.OrdinalIgnoreCase))
            return audio ? AudioTag(path, f => f.Tag.Year > 0
                ? string.Format(Translations.Get("Decade", idioma), f.Tag.Year / 10 * 10) : null) : null;
        if (string.Equals(key, "Año", StringComparison.OrdinalIgnoreCase))
            return audio ? AudioTag(path, f => f.Tag.Year > 0 ? f.Tag.Year.ToString() : null)
                : image ? ImageYear(path) : video || doc ? FileYear(path) : null;

        return null; // Criterios sin fuente de metadatos local (Director, Saga, Cadena, Autor, Editorial).
    }

    private static string? AudioTag(string path, Func<TagLib.File, string?> get)
    {
        try
        {
            using var f = TagLib.File.Create(path);
            return Clean(get(f));
        }
        catch (Exception) { return null; }
    }

    /// <summary>Normaliza una etiqueta de género: recorta, colapsa espacios, separa géneros
    /// compuestos ("Electronic - Pop - New Wave - Synth") y traduce alias comunes.</summary>
    internal static string? NormalizeGenre(string? raw, Idioma idioma)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        string s = Regex.Replace(raw.Trim(), @"\s+", " ");
        string tag = Regex.Split(s, @"\s*(?:-|;|,|/|\|)\s*")
            .Select(p => p.Trim())
            .FirstOrDefault(p => p.Length > 0) ?? s;
        if (GenreMap.TryGetValue(tag, out string? mapped))
            return mapped switch
            {
                "Otros" => Translations.Get("Otros", idioma),
                "Banda sonora" => Translations.Get("CatSoundtrack", idioma),
                _ => mapped
            };
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(tag.ToLowerInvariant());
    }

    private static string? FileYear(string path)
    {
        try
        {
            return File.Exists(path) ? File.GetLastWriteTime(path).Year.ToString() : null;
        }
        catch (Exception) { return null; }
    }

    private static string? ImageYear(string path)
    {
        try
        {
            var dirs = MetadataExtractor.ImageMetadataReader.ReadMetadata(path);
            var sub = dirs.OfType<MetadataExtractor.Formats.Exif.ExifSubIfdDirectory>().FirstOrDefault();
            if (sub != null &&
                MetadataExtractor.DirectoryExtensions.TryGetDateTime(sub, MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagDateTimeOriginal, out DateTime dt))
                return dt.Year.ToString();
        }
        catch (Exception) { }
        return FileYear(path);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? First(string[] values) =>
        values?.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}