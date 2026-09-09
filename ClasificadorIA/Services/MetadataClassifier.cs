using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

/// <summary>Extrae categorías desde los metadatos reales del archivo (EXIF, tags de audio, fecha). Fallback: null.</summary>
public static class MetadataClassifier
{
    public static string? TryClassify(string path, string? criterionKey, Idioma idioma)
    {
        string ext = Path.GetExtension(path);
        if (LocalClassifier.IsImageExt(ext)) return ImageDate(path, idioma);
        if (LocalClassifier.IsAudioExt(ext)) return AudioTag(path, criterionKey, idioma);
        if (LocalClassifier.IsVideoExt(ext)) return VideoCategory(path, idioma);
        if (LocalClassifier.IsDocExt(ext)) return FileYear(path);
        return null;
    }

    private static string? ImageDate(string path, Idioma idioma)
    {
        try
        {
            var dirs = MetadataExtractor.ImageMetadataReader.ReadMetadata(path);
            var sub = dirs.OfType<MetadataExtractor.Formats.Exif.ExifSubIfdDirectory>().FirstOrDefault();
            if (sub == null) return null;
            if (!MetadataExtractor.DirectoryExtensions.TryGetDateTime(sub, MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagDateTimeOriginal, out DateTime dt))
                return null;
            string month = CultureInfo
                .GetCultureInfo(idioma == Idioma.Español ? "es" : "en")
                .DateTimeFormat
                .GetMonthName(dt.Month);
            return $"{month} {dt.Year}";
        }
        catch (Exception) { return null; }
    }

    private static string? AudioTag(string path, string? criterionKey, Idioma idioma)
    {
        try
        {
            using var f = TagLib.File.Create(path);
            return criterionKey switch
            {
                "Artista" => First(f.Tag.Performers),
                "Álbum" => string.IsNullOrWhiteSpace(f.Tag.Album) ? null : f.Tag.Album,
                "Año" => f.Tag.Year > 0 ? f.Tag.Year.ToString() : null,
                "Época" => f.Tag.Year > 0 ? string.Format(Translations.Get("Decade", idioma), f.Tag.Year / 10 * 10) : null,
                _ => First(f.Tag.Genres) ?? First(f.Tag.Performers)
            };
        }
        catch (Exception) { return null; }
    }

    private static string? VideoCategory(string path, Idioma idioma)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        var m = Regex.Match(name, @"[Ss](\d{1,2})[Ee]\d{1,2}");
        if (m.Success)
            return string.Format(Translations.Get("Temporada", idioma), int.Parse(m.Groups[1].Value));
        return FileYear(path);
    }

    private static string? FileYear(string path)
    {
        try
        {
            return File.Exists(path) ? File.GetLastWriteTime(path).Year.ToString() : null;
        }
        catch (Exception) { return null; }
    }

    private static string? First(string[] values) =>
        values?.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}