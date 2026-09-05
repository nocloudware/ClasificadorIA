namespace ClasificadorIA.Models;

public sealed record ClassificationMode(string Key, string DescripcionEs, string DescripcionEn, string[] Criterios);

public static class ClassificationModes
{
    public static readonly IReadOnlyList<ClassificationMode> All = new ClassificationMode[]
    {
        new("Genérico", "archivos de cualquier tipo", "files of any kind", new[] { "Tema" }),
        new("Música", "canciones", "songs", new[] { "Género", "Artista", "Época", "Álbum" }),
        new("Películas", "películas", "movies", new[] { "Género", "Director", "Año", "Saga" }),
        new("Series", "series", "tv shows", new[] { "Género", "Cadena", "Año", "Saga" }),
        new("Libros", "libros", "books", new[] { "Género", "Autor", "Año", "Editorial" })
    };

    public static ClassificationMode Default => All[0];

    public static ClassificationMode? Find(string key)
    {
        foreach (var m in All)
            if (string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase))
                return m;
        return null;
    }

    private static readonly Dictionary<string, (string Es, string En)> Criterion = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Tema"] = ("Tema", "Topic"),
        ["Género"] = ("Género", "Genre"),
        ["Artista"] = ("Artista", "Artist"),
        ["Época"] = ("Época", "Era"),
        ["Álbum"] = ("Álbum", "Album"),
        ["Director"] = ("Director", "Director"),
        ["Año"] = ("Año", "Year"),
        ["Saga"] = ("Saga", "Saga"),
        ["Cadena"] = ("Cadena", "Network"),
        ["Autor"] = ("Autor", "Author"),
        ["Editorial"] = ("Editorial", "Publisher")
    };

    public static IReadOnlyList<string> GetCriteria(ClassificationMode mode, Idioma idioma) =>
        mode.Criterios.Select(c => TranslateCriterion(c, idioma)).ToList();

    public static string TranslateCriterion(string criterion, Idioma idioma) =>
        Criterion.TryGetValue(criterion, out var pair) ? (idioma == Idioma.Español ? pair.Es : pair.En) : criterion;

    public static string GetCriterionKey(string translated, Idioma idioma)
    {
        foreach (var kv in Criterion)
        {
            string match = idioma == Idioma.Español ? kv.Value.Es : kv.Value.En;
            if (string.Equals(match, translated, StringComparison.OrdinalIgnoreCase))
                return kv.Key;
        }
        return translated;
    }
}