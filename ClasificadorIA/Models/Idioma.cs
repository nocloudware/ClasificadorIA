namespace ClasificadorIA.Models;

public enum Idioma { Español, Inglés, Francés, Alemán, Portugués, Italiano, Japonés, Chino }

public static class LanguageHelpers
{
    public static string CultureCode(this Idioma idioma) => idioma switch
    {
        Idioma.Español => "es",
        Idioma.Inglés => "en",
        Idioma.Francés => "fr",
        Idioma.Alemán => "de",
        Idioma.Portugués => "pt",
        Idioma.Italiano => "it",
        Idioma.Japonés => "ja",
        Idioma.Chino => "zh",
        _ => "en"
    };

    public static Idioma FromCulture(string? culture) => culture?.ToLowerInvariant() switch
    {
        "es" => Idioma.Español,
        "en" => Idioma.Inglés,
        "fr" => Idioma.Francés,
        "de" => Idioma.Alemán,
        "pt" => Idioma.Portugués,
        "it" => Idioma.Italiano,
        "ja" => Idioma.Japonés,
        "zh" => Idioma.Chino,
        _ => Idioma.Español
    };
}