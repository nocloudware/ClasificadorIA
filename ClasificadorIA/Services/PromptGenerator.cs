using System.Text;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

public static class PromptGenerator
{
    private const string TemplateEs = @"Eres un experto en clasificación de {0}.
Clasifica estos archivos por **{1}** según su nombre.
Intenta generar aproximadamente {2} categorías.

Archivos:
{3}

Devuelve SOLO este JSON:
{{
  ""categorias"": {{
    ""Categoría1"": [""archivo1"", ""archivo2""],
    ""Categoría2"": [""archivo3""]
  }}
}}";

    private const string TemplateEn = @"You are an expert in classifying {0}.
Classify these files by **{1}** based on their name.
Try to generate approximately {2} categories.

Files:
{3}

Return ONLY this JSON:
{{
  ""categories"": {{
    ""Category1"": [""file1"", ""file2""],
    ""Category2"": [""file3""]
  }}
}}";

    public static string Generate(ClassificationMode mode, string criterionKey, int depth, Idioma idioma, IEnumerable<string> files)
    {
        if (mode is null) throw new ArgumentNullException(nameof(mode));
        if (depth <= 0) depth = 5;

        string descripcion = idioma == Idioma.Español ? mode.DescripcionEs : mode.DescripcionEn;
        string criterio = ClassificationModes.TranslateCriterion(criterionKey, idioma).ToLowerInvariant();

        var archivos = new StringBuilder();
        foreach (var f in files)
            archivos.Append("- ").Append(f).Append('\n');

        string plantilla = idioma == Idioma.Español ? TemplateEs : TemplateEn;
        return string.Format(plantilla, descripcion, criterio, depth, archivos.ToString().TrimEnd('\n'));
    }
}