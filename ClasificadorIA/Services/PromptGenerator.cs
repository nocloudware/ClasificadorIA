using System.Collections.Generic;
using System.Text;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

public static class PromptGenerator
{
    private static readonly Dictionary<string, string> AssignmentTemplate = new()
    {
        ["es"] = @"Eres un experto en clasificación de {0}.
Asigna a CADA archivo la categoría que mejor lo describa según **{1}**, basándote solo en su nombre.
Usa tantas categorías distintas como necesites; no las agrupes a mano.
{2}Archivos (separados uno por línea):
{3}

Responde SOLO con JSON válido, sin texto adicional:
{{
  ""archivos"": [
    {{ ""archivo"": ""nombre"", ""categoria"": ""Categoria"" }}
  ]
}}",
        ["en"] = @"You are an expert in classifying {0}.
Assign each file the category that best describes it by **{1}**, based only on its name.
Use as many distinct categories as needed; do not group them by hand.
{2}Files (one per line):
{3}

Reply with ONLY valid JSON, no extra text:
{{
  ""files"": [
    {{ ""file"": ""name"", ""category"": ""Category"" }}
  ]
}}",
        ["fr"] = @"Vous êtes un expert en classification de {0}.
Attribuez à CHAQUE fichier la catégorie qui le décrit le mieux selon **{1}**, en vous basant uniquement sur son nom.
Utilisez autant de catégories distinctes que nécessaire ; ne les regroupez pas à la main.
{2}Fichiers (un par ligne) :
{3}

Répondez UNIQUEMENT avec un JSON valide, sans texte supplémentaire :
{{
  ""fichiers"": [
    {{ ""fichier"": ""nom"", ""categorie"": ""Categorie"" }}
  ]
}}",
        ["de"] = @"Sie sind ein Experte für die Klassifizierung von {0}.
Weisen Sie JEDER Datei die Kategorie zu, die sie am besten beschreibt, basierend nur auf ihrem Namen, nach **{1}**.
Verwenden Sie so viele verschiedene Kategorien wie nötig; gruppieren Sie sie nicht von Hand.
{2}Dateien (eine pro Zeile):
{3}

Antworten Sie NUR mit gültigem JSON, ohne zusätzlichen Text:
{{
  ""dateien"": [
    {{ ""datei"": ""name"", ""kategorie"": ""Kategorie"" }}
  ]
}}",
        ["pt"] = @"Você é um especialista em classificação de {0}.
Atribua a CADA arquivo a categoria que melhor o descreva de acordo com **{1}**, baseando-se apenas no nome.
Use quantas categorias distintas forem necessárias; não as agrupe manualmente.
{2}Arquivos (um por linha):
{3}

Responda APENAS com JSON válido, sem texto adicional:
{{
  ""arquivos"": [
    {{ ""arquivo"": ""nome"", ""categoria"": ""Categoria"" }}
  ]
}}",
        ["it"] = @"Sei un esperto nella classificazione di {0}.
Assegna a CIASCUN file la categoria che meglio lo descrive secondo **{1}**, basandoti solo sul nome.
Usa tutte le categorie distinte che servono; non raggrupparle a mano.
{2}File (uno per riga):
{3}

Rispondi SOLO con JSON valido, senza testo aggiuntivo:
{{
  ""file"": [
    {{ ""file"": ""nome"", ""categoria"": ""Categoria"" }}
  ]
}}",
        ["ja"] = @"あなたは{0}の分類の専門家です。
各ファイルに、**{1}**に基づき、名前だけを見て最も適切なカテゴリを割り当ててください。
必要なだけ異なるカテゴリを使ってください。手動でまとめないでください。
{2}ファイル（1行に1つ）:
{3}

追加のテキストなしで、有効な JSON のみで回答してください:
{{
  ""ファイル"": [
    {{ ""ファイル"": ""名前"", ""カテゴリ"": ""カテゴリ"" }}
  ]
}}",
        ["zh"] = @"您是{0}分类方面的专家。
请根据**{1}**，仅基于文件名，为每个文件分配最合适的分类。
请使用尽可能多的不同分类；请勿手动归类。
{2}文件（每行一个）：
{3}

请只回复有效的 JSON，不要附加任何文字：
{{
  ""文件"": [
    {{ ""文件"": ""名称"", ""分类"": ""分类"" }}
  ]
}}",
    };

    // Guía opcional: categorías ya detectadas en lotes anteriores (solo referencia, no obligación).
    private static readonly Dictionary<string, string> KnownCategories = new()
    {
        ["es"] = "Estas categorías ya se usaron en análisis anteriores; úsalas cuando encajen, o agrega nuevas si hace falta:\n{0}\n",
        ["en"] = "These categories were already used in previous analyses; reuse them when they fit, or add new ones if needed:\n{0}\n",
        ["fr"] = "Ces catégories ont déjà été utilisées lors d'analyses précédentes ; réutilisez-les si elles conviennent, ou ajoutez-en de nouvelles si nécessaire :\n{0}\n",
        ["de"] = "Diese Kategorien wurden in früheren Analysen bereits verwendet; verwenden Sie sie, wenn sie passen, oder fügen Sie bei Bedarf neue hinzu:\n{0}\n",
        ["pt"] = "Estas categorias já foram usadas em análises anteriores; use-as quando encaixarem ou adicione novas se precisar:\n{0}\n",
        ["it"] = "Queste categorie sono già state usate nelle analisi precedenti; riusale se vanno bene, o aggiungine di nuove se serve:\n{0}\n",
        ["ja"] = "これらのカテゴリは以前の分析で既に使用されています。適していれば再利用し、必要なら新しいものを追加してください:\n{0}\n",
        ["zh"] = "这些分类在之前的分析中已使用过；如果合适请复用，如有需要也可添加新分类：\n{0}\n",
    };

    private static readonly Dictionary<string, string> ConsolidationTemplate = new()
    {
        ["es"] = @"Recibí estas categorías preliminares asignadas a un grupo de archivos, con su cantidad y ejemplos:
{0}

Consolídalas en aproximadamente {1} categorías finales, agrupando las similares.
Responde SOLO con un JSON donde cada clave es una categoría final y su valor es la lista de categorías preliminares que incluye:

{{
  ""finales"": {{
    ""Final1"": [""cat1"", ""cat2""],
    ""Final2"": [""cat3""]
  }}
}}",
        ["en"] = @"I received these preliminary categories assigned to a group of files, with their count and examples:
{0}

Consolidate them into approximately {1} final categories, grouping similar ones.
Reply with ONLY a JSON where each key is a final category and its value is the list of preliminary categories it includes:

{{
  ""final"": {{
    ""Final1"": [""cat1"", ""cat2""],
    ""Final2"": [""cat3""]
  }}
}}",
        ["fr"] = @"J'ai reçu ces catégories préliminaires attribuées à un groupe de fichiers, avec leur quantité et des exemples :
{0}

Consolidez-les en environ {1} catégories finales, en regroupant les similaires.
Répondez UNIQUEMENT avec un JSON où chaque clé est une catégorie finale et sa valeur est la liste des catégories préliminaires qu'elle inclut :

{{
  ""finaux"": {{
    ""Final1"": [""cat1"", ""cat2""],
    ""Final2"": [""cat3""]
  }}
}}",
        ["de"] = @"Ich habe diese vorläufigen Kategorien erhalten, die einer Gruppe von Dateien zugeordnet wurden, mit Anzahl und Beispielen:
{0}

Fassen Sie sie zu ungefähr {1} endgültigen Kategorien zusammen, indem Sie ähnliche gruppieren.
Antworten Sie NUR mit einem JSON, in dem jeder Schlüssel eine endgültige Kategorie ist und sein Wert die Liste der enthaltenen vorläufigen Kategorien:

{{
  ""final"": {{
    ""Final1"": [""cat1"", ""cat2""],
    ""Final2"": [""cat3""]
  }}
}}",
        ["pt"] = @"Recebi estas categorias preliminares atribuídas a um grupo de arquivos, com quantidade e exemplos:
{0}

Consolide-as em aproximadamente {1} categorias finais, agrupando as similares.
Responda APENAS com um JSON em que cada chave é uma categoria final e seu valor é a lista de categorias preliminares que inclui:

{{
  ""finais"": {{
    ""Final1"": [""cat1"", ""cat2""],
    ""Final2"": [""cat3""]
  }}
}}",
        ["it"] = @"Ho ricevuto queste categorie preliminari assegnate a un gruppo di file, con quantità ed esempi:
{0}

Consolidale in circa {1} categorie finali, raggruppando quelle simili.
Rispondi SOLO con un JSON in cui ogni chiave è una categoria finale e il suo valore è la lista delle categorie preliminari che include:

{{
  ""finali"": {{
    ""Final1"": [""cat1"", ""cat2""],
    ""Final2"": [""cat3""]
  }}
}}",
        ["ja"] = @"ファイルのグループに割り当てられた、数量と例付きの予備カテゴリを受け取りました:
{0}

似たものをまとめて、約{1}個の最終カテゴリに統合してください。
各キーが最終カテゴリで、その値が含まれる予備カテゴリのリストである JSON のみで回答してください:

{{
  ""最終"": {{
    ""Final1"": [""cat1"", ""cat2""],
    ""Final2"": [""cat3""]
  }}
}}",
        ["zh"] = @"我收到了分配给一组文件的初步分类，附有数量和示例：
{0}

请将它们整合为大约 {1} 个最终分类，将相似的归类。
请只回复一个 JSON，其中每个键是一个最终分类，其值是该分类包含的初步分类列表：

{{
  ""最终"": {{
    ""Final1"": [""cat1"", ""cat2""],
    ""Final2"": [""cat3""]
  }}
}}",
    };

    private static readonly Dictionary<string, (string Files, string Example)> ConsolidateWords = new()
    {
        ["es"] = ("archivos", "ej"),
        ["en"] = ("files", "e.g."),
        ["fr"] = ("fichiers", "ex"),
        ["de"] = ("Dateien", "z.B."),
        ["pt"] = ("arquivos", "ex"),
        ["it"] = ("file", "es."),
        ["ja"] = ("ファイル", "例"),
        ["zh"] = ("个文件", "例"),
    };

    // Lote especial: archivos que quedaron en "Otros", forzados a una categoría ya definida.
    private static readonly Dictionary<string, string> ReassignTemplate = new()
    {
        ["es"] = @"Estos archivos no encajaron en el análisis anterior. Asigna CADA uno a la categoría más adecuada de esta lista (son las únicas permitidas):
{0}

No crees categorías nuevas y no los dejes en ""Otros"": elige siempre la categoría más cercana.
Archivos:
{1}

Responde SOLO con JSON válido, sin texto adicional:
{{
  ""archivos"": [
    {{ ""archivo"": ""nombre"", ""categoria"": ""Categoria"" }}
  ]
}}",
        ["en"] = @"These files did not fit the previous analysis. Assign EACH one to the most suitable category from this list (these are the only allowed ones):
{0}

Do not create new categories and do not leave them in ""Others"": always pick the closest category.
Files:
{1}

Reply with ONLY valid JSON, no extra text:
{{
  ""files"": [
    {{ ""file"": ""name"", ""category"": ""Category"" }}
  ]
}}",
        ["fr"] = @"Ces fichiers ne correspondaient pas à l'analyse précédente. Attribuez à CHACUN la catégorie la plus appropriée de cette liste (ce sont les seules autorisées) :
{0}

Ne créez pas de nouvelles catégories et ne les laissez pas dans « Autres » : choisissez toujours la plus proche.
Fichiers :
{1}

Répondez UNIQUEMENT avec un JSON valide, sans texte supplémentaire :
{{
  ""fichiers"": [
    {{ ""fichier"": ""nom"", ""categorie"": ""Categorie"" }}
  ]
}}",
        ["de"] = @"Diese Dateien passten nicht zur vorherigen Analyse. Weisen Sie JEDER die am besten passende Kategorie aus dieser Liste zu (nur diese sind erlaubt):
{0}

Erstellen Sie keine neuen Kategorien und lassen Sie sie nicht in ""Andere"": Wählen Sie immer die nächstgelegene.
Dateien:
{1}

Antworten Sie NUR mit gültigem JSON, ohne zusätzlichen Text:
{{
  ""dateien"": [
    {{ ""datei"": ""name"", ""kategorie"": ""Kategorie"" }}
  ]
}}",
        ["pt"] = @"Estes arquivos não se encaixaram na análise anterior. Atribua a CADA um a categoria mais adequada desta lista (são as únicas permitidas):
{0}

Não crie categorias novas e não os deixe em ""Outros"": escolha sempre a mais próxima.
Arquivos:
{1}

Responda APENAS com JSON válido, sem texto adicional:
{{
  ""arquivos"": [
    {{ ""arquivo"": ""nome"", ""categoria"": ""Categoria"" }}
  ]
}}",
        ["it"] = @"Questi file non rientravano nell'analisi precedente. Assegna a CIASCUNO la categoria più adatta di questo elenco (sono le uniche consentite):
{0}

Non creare nuove categorie e non lasciarli in ""Altri"": scegli sempre la più vicina.
File:
{1}

Rispondi SOLO con JSON valido, senza testo aggiuntivo:
{{
  ""file"": [
    {{ ""file"": ""nome"", ""categoria"": ""Categoria"" }}
  ]
}}",
        ["ja"] = @"これらのファイルは前回の分析に当てはまりませんでした。次のリスト（これらが唯一の候補です）から、各ファイルに最も適切なカテゴリを割り当ててください:
{0}

新しいカテゴリを作成せず、「その他」にも残さないでください。必ず最も近いものを選んでください。
ファイル:
{1}

追加のテキストなしで、有効な JSON のみで回答してください:
{{
  ""ファイル"": [
    {{ ""ファイル"": ""名前"", ""カテゴリ"": ""カテゴリ"" }}
  ]
}}",
        ["zh"] = @"这些文件在上次分析中没有匹配项。请将每个文件分配到此列表中最合适的分类（仅限这些）：
{0}

请勿创建新分类，也不要将它们留在“其他”中：请始终选择最接近的分类。
文件：
{1}

请只回复有效的 JSON，不要附加任何文字：
{{
  ""文件"": [
    {{ ""文件"": ""名称"", ""分类"": ""分类"" }}
  ]
}}",
    };

    public static string AssignmentBatch(
        ClassificationMode mode, string criterionKey, Idioma idioma, IEnumerable<string> files,
        IEnumerable<string>? knownCategories = null)
    {
        if (mode is null) throw new ArgumentNullException(nameof(mode));

        string culture = idioma.CultureCode();
        string descripcion = mode.Description(idioma);
        string criterio = ClassificationModes.TranslateCriterion(criterionKey, idioma).ToLowerInvariant();

        var archivos = new StringBuilder();
        foreach (var f in files)
            archivos.Append("- ").Append(f).Append('\n');

        string knownBlock = knownCategories == null ? "" : BuildKnownBlock(knownCategories.ToList(), culture);
        string plantilla = AssignmentTemplate[idioma.CultureCode()];
        return string.Format(plantilla, descripcion, criterio, knownBlock, archivos.ToString().TrimEnd('\n'));
    }

    private static string BuildKnownBlock(List<string> categories, string culture)
    {
        var distinct = categories.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (distinct.Count == 0) return "";

        var listado = new StringBuilder();
        listado.Append("- ").Append(string.Join("\n- ", distinct)).Append('\n');
        return string.Format(KnownCategories[culture], listado.ToString().TrimEnd('\n'));
    }

    /// <summary>Lote especial: reasigna los archivos de "Otros" a una de las categorías finales ya definidas.</summary>
    public static string ReassignOthers(IReadOnlyList<string> categories, IEnumerable<string> files, Idioma idioma)
    {
        string culture = idioma.CultureCode();
        var listado = new StringBuilder();
        foreach (var c in categories)
            listado.Append("- ").Append(c).Append('\n');
        var archivos = new StringBuilder();
        foreach (var f in files)
            archivos.Append("- ").Append(f).Append('\n');

        return string.Format(
            ReassignTemplate[culture],
            listado.ToString().TrimEnd('\n'),
            archivos.ToString().TrimEnd('\n'));
    }

    public static string Consolidate(IReadOnlyList<CategoryStat> categories, int depth, Idioma idioma)
    {
        if (categories.Count == 0) return "";
        if (depth <= 0) depth = 5;

        string culture = idioma.CultureCode();
        var words = ConsolidateWords[culture];

        var stats = new StringBuilder();
        foreach (var c in categories)
        {
            stats.Append("- ").Append(c.Name).Append(" (").Append(c.Count).Append(' ').Append(words.Files);
            if (c.Examples.Length > 0) stats.Append(", ").Append(words.Example).Append(": ").Append(string.Join(", ", c.Examples.Take(2)));
            stats.Append(")\n");
        }

        return string.Format(ConsolidationTemplate[culture], stats.ToString().TrimEnd('\n'), depth);
    }
}