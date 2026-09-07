using System.Text;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

public static class PromptGenerator
{
    private const string AssignmentTemplateEs = @"Eres un experto en clasificación de {0}.
Asigna a CADA archivo la categoría que mejor lo describa según **{1}**, basándote solo en su nombre.
Usa tantas categorías distintas como necesites; no las agrupes a mano.

Archivos (separados uno por línea):
{2}

Responde SOLO con JSON válido, sin texto adicional:
{{
  ""archivos"": [
    {{ ""archivo"": ""nombre"", ""categoria"": ""Categoria"" }}
  ]
}}";

    private const string AssignmentTemplateEn = @"You are an expert in classifying {0}.
Assign each file the category that best describes it by **{1}**, based only on its name.
Use as many distinct categories as needed; do not group them by hand.

Files (one per line):
{2}

Reply with ONLY valid JSON, no extra text:
{{
  ""files"": [
    {{ ""file"": ""name"", ""category"": ""Category"" }}
  ]
}}";

    private const string ConsolidationTemplateEs = @"Recibí estas categorías preliminares asignadas a un grupo de archivos, con su cantidad y ejemplos:
{0}

Consolídalas en aproximadamente {1} categorías finales, agrupando las similares.
Responde SOLO con un JSON donde cada clave es una categoría final y su valor es la lista de categorías preliminares que incluye:

{{
  ""finales"": {{
    ""Final1"": [""cat1"", ""cat2""],
    ""Final2"": [""cat3""]
  }}
}}";

    private const string ConsolidationTemplateEn = @"I received these preliminary categories assigned to a group of files, with their count and examples:
{0}

Consolidate them into approximately {1} final categories, grouping similar ones.
Reply with ONLY a JSON where each key is a final category and its value is the list of preliminary categories it includes:

{{
  ""final"": {{
    ""Final1"": [""cat1"", ""cat2""],
    ""Final2"": [""cat3""]
  }}
}}";

    public static string AssignmentBatch(ClassificationMode mode, string criterionKey, Idioma idioma, IEnumerable<string> files)
    {
        if (mode is null) throw new ArgumentNullException(nameof(mode));

        string descripcion = idioma == Idioma.Español ? mode.DescripcionEs : mode.DescripcionEn;
        string criterio = ClassificationModes.TranslateCriterion(criterionKey, idioma).ToLowerInvariant();

        var archivos = new StringBuilder();
        foreach (var f in files)
            archivos.Append("- ").Append(f).Append('\n');

        string plantilla = idioma == Idioma.Español ? AssignmentTemplateEs : AssignmentTemplateEn;
        return string.Format(plantilla, descripcion, criterio, archivos.ToString().TrimEnd('\n'));
    }

    public static string Consolidate(IReadOnlyList<CategoryStat> categories, int depth, Idioma idioma)
    {
        if (categories.Count == 0) return "";
        if (depth <= 0) depth = 5;

        bool es = idioma == Idioma.Español;
        string countWord = es ? "archivos" : "files";
        string exampleWord = es ? "ej" : "e.g.";

        var stats = new StringBuilder();
        foreach (var c in categories)
        {
            stats.Append("- ").Append(c.Name).Append(" (").Append(c.Count).Append(' ').Append(countWord);
            if (c.Examples.Length > 0) stats.Append(", ").Append(exampleWord).Append(": ").Append(string.Join(", ", c.Examples.Take(2)));
            stats.Append(")\n");
        }

        string plantilla = idioma == Idioma.Español ? ConsolidationTemplateEs : ConsolidationTemplateEn;
        return string.Format(plantilla, stats.ToString().TrimEnd('\n'), depth);
    }
}