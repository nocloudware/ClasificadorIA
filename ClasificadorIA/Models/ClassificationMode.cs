using System.Collections.Generic;
using System.Linq;

namespace ClasificadorIA.Models;

public sealed record ClassificationMode(string Key, string[] Criterios, IReadOnlyDictionary<string, string> Descripciones)
{
    public string Description(Idioma idioma) =>
        Descripciones.TryGetValue(idioma.CultureCode(), out var d) ? d : Descripciones["en"];
}

public static class ClassificationModes
{
    public static readonly IReadOnlyList<ClassificationMode> All = new ClassificationMode[]
    {
        new(
            "Genérico",
            new[] { "Tema" },
            new Dictionary<string, string>
            {
                ["es"] = "archivos de cualquier tipo", ["en"] = "files of any kind",
                ["fr"] = "fichiers de tout type", ["de"] = "Dateien jeder Art",
                ["pt"] = "arquivos de qualquer tipo", ["it"] = "file di qualsiasi tipo",
                ["ja"] = "あらゆる種類のファイル", ["zh"] = "任意类型的文件",
            }),
        new(
            "Música",
            new[] { "Género", "Artista", "Época", "Álbum" },
            new Dictionary<string, string>
            {
                ["es"] = "canciones", ["en"] = "songs",
                ["fr"] = "chansons", ["de"] = "Lieder",
                ["pt"] = "canções", ["it"] = "canzoni",
                ["ja"] = "曲", ["zh"] = "歌曲",
            }),
        new(
            "Películas",
            new[] { "Género", "Director", "Año", "Saga" },
            new Dictionary<string, string>
            {
                ["es"] = "películas", ["en"] = "movies",
                ["fr"] = "films", ["de"] = "Filme",
                ["pt"] = "filmes", ["it"] = "film",
                ["ja"] = "映画", ["zh"] = "电影",
            }),
        new(
            "Series",
            new[] { "Género", "Cadena", "Año", "Saga" },
            new Dictionary<string, string>
            {
                ["es"] = "series", ["en"] = "tv shows",
                ["fr"] = "séries", ["de"] = "Serien",
                ["pt"] = "séries", ["it"] = "serie TV",
                ["ja"] = "テレビドラマ", ["zh"] = "剧集",
            }),
        new(
            "Libros",
            new[] { "Género", "Autor", "Año", "Editorial" },
            new Dictionary<string, string>
            {
                ["es"] = "libros", ["en"] = "books",
                ["fr"] = "livres", ["de"] = "Bücher",
                ["pt"] = "livros", ["it"] = "libri",
                ["ja"] = "本", ["zh"] = "书籍",
            }),
    };

    public static ClassificationMode Default => All[0];

    public static ClassificationMode? Find(string key)
    {
        foreach (var m in All)
            if (string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase))
                return m;
        return null;
    }

    private static readonly Dictionary<string, Dictionary<string, string>> Criterion = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Tema"] = new() { ["es"] = "Tema", ["en"] = "Topic", ["fr"] = "Thème", ["de"] = "Thema", ["pt"] = "Tema", ["it"] = "Tema", ["ja"] = "テーマ", ["zh"] = "主题" },
        ["Género"] = new() { ["es"] = "Género", ["en"] = "Genre", ["fr"] = "Genre", ["de"] = "Genre", ["pt"] = "Gênero", ["it"] = "Genere", ["ja"] = "ジャンル", ["zh"] = "类型" },
        ["Artista"] = new() { ["es"] = "Artista", ["en"] = "Artist", ["fr"] = "Artiste", ["de"] = "Künstler", ["pt"] = "Artista", ["it"] = "Artista", ["ja"] = "アーティスト", ["zh"] = "艺术家" },
        ["Época"] = new() { ["es"] = "Época", ["en"] = "Era", ["fr"] = "Époque", ["de"] = "Ära", ["pt"] = "Época", ["it"] = "Epoca", ["ja"] = "年代", ["zh"] = "年代" },
        ["Álbum"] = new() { ["es"] = "Álbum", ["en"] = "Album", ["fr"] = "Album", ["de"] = "Album", ["pt"] = "Álbum", ["it"] = "Album", ["ja"] = "アルバム", ["zh"] = "专辑" },
        ["Director"] = new() { ["es"] = "Director", ["en"] = "Director", ["fr"] = "Réalisateur", ["de"] = "Regisseur", ["pt"] = "Diretor", ["it"] = "Regista", ["ja"] = "監督", ["zh"] = "导演" },
        ["Año"] = new() { ["es"] = "Año", ["en"] = "Year", ["fr"] = "Année", ["de"] = "Jahr", ["pt"] = "Ano", ["it"] = "Anno", ["ja"] = "年", ["zh"] = "年份" },
        ["Saga"] = new() { ["es"] = "Saga", ["en"] = "Saga", ["fr"] = "Saga", ["de"] = "Saga", ["pt"] = "Saga", ["it"] = "Saga", ["ja"] = "シリーズ", ["zh"] = "系列" },
        ["Cadena"] = new() { ["es"] = "Cadena", ["en"] = "Network", ["fr"] = "Chaîne", ["de"] = "Sender", ["pt"] = "Emissora", ["it"] = "Rete", ["ja"] = "放送局", ["zh"] = "电视台" },
        ["Autor"] = new() { ["es"] = "Autor", ["en"] = "Author", ["fr"] = "Auteur", ["de"] = "Autor", ["pt"] = "Autor", ["it"] = "Autore", ["ja"] = "著者", ["zh"] = "作者" },
        ["Editorial"] = new() { ["es"] = "Editorial", ["en"] = "Publisher", ["fr"] = "Maison d'édition", ["de"] = "Verlag", ["pt"] = "Editora", ["it"] = "Casa editrice", ["ja"] = "出版社", ["zh"] = "出版社" },
    };

    public static IReadOnlyList<string> GetCriteria(ClassificationMode mode, Idioma idioma) =>
        mode.Criterios.Select(c => TranslateCriterion(c, idioma)).ToList();

    public static string TranslateCriterion(string criterion, Idioma idioma) =>
        Criterion.TryGetValue(criterion, out var langs)
            ? (langs.TryGetValue(idioma.CultureCode(), out var v) ? v : langs["en"])
            : criterion;

    public static string GetCriterionKey(string translated, Idioma idioma)
    {
        foreach (var kv in Criterion)
        {
            string match = TranslateCriterion(kv.Key, idioma);
            if (string.Equals(match, translated, StringComparison.OrdinalIgnoreCase))
                return kv.Key;
        }
        return translated;
    }
}