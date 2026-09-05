using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ClasificadorIA.Models;
using NoCloudware.UI.Core.Controls;

namespace ClasificadorIA.Services;

public static class SelfTest
{
    private static readonly List<string> Failures = new();

    public static bool Run()
    {
        Failures.Clear();
        RunPromptGenerator();
        RunResponseParser();
        RunFileOrganizer();
        RunFileFilters();
        RunTranslations();
        RunClassificationModes();
        RunByokConfigStore();
        RunAiClientOffline();
        RunFileListCustomContent();

        if (Failures.Count == 0)
        {
            Console.WriteLine("SELF-TEST: OK");
            return true;
        }
        Console.WriteLine("SELF-TEST: " + Failures.Count + " fallo(s)");
        foreach (var f in Failures)
            Console.WriteLine("  ✗ " + f);
        return false;
    }

    private static void Assert(string label, bool condition, string? detail = null)
    {
        if (!condition)
            Failures.Add(label + (detail != null ? ": " + detail : ""));
    }

    // ── Prompts ────────────────────────────────────────────────────────

    private static void RunPromptGenerator()
    {
        var mode = ClassificationModes.Find("Música")!;
        var files = new[] { "cancion1.mp3", "cancion2.mp3" };

        string es = PromptGenerator.Generate(mode, "Género", 5, Idioma.Español, files);
        Assert("Prompt ES contiene criterio", es.Contains("género"));
        Assert("Prompt ES contiene key JSON", es.Contains("\"categorias\""));
        Assert("Prompt ES lista archivos", es.Contains("- cancion1.mp3"));

        string en = PromptGenerator.Generate(mode, "Género", 5, Idioma.Inglés, files);
        Assert("Prompt EN contiene criterio", en.Contains("genre"));
        Assert("Prompt EN contiene key JSON", en.Contains("\"categories\""));
        Assert("Prompt EN usa descripcion EN", en.Contains("classifying songs"));
    }

    // ── Parser ─────────────────────────────────────────────────────────

    private static void RunResponseParser()
    {
        var realFiles = new[] { "album1.mp3", "Red Hot Chili Peppers - Under The Bridge.mp3", "Bad Bunny - X.mp3", "The Beatles - Hey Jude.mp3" };

        string es = """{"categorias": {"Rock": ["album1.MP3"], "Reggaetón": ["bad bunny - x.mp3"]}, "extra": 1}""";
        var esResult = ResponseParser.Parse(es, realFiles, Idioma.Español);
        Assert("Parse ES encuentra categorías", esResult.Count == 2, $"got {esResult.Count}");
        Assert("Parse ES matchea case-insensitive", esResult.Any(r => r.Category == "Rock" && r.Files[0] == "album1.mp3"));

        string en = "```json\n{\"categories\": {\"Rock\": [\"Red Hot Chili Peppers - Under The Bridge.mp3\"]}}\n```";
        var enResult = ResponseParser.Parse(en, realFiles, Idioma.Inglés);
        Assert("Parse EN con código markdown", enResult.Count == 1 && enResult[0].Files[0] == "Red Hot Chili Peppers - Under The Bridge.mp3");

        string fallback = """{"categories": {"Beatles": ["The Beatles - Hey Jude.mp3"]}}""";
        var fbResult = ResponseParser.Parse(fallback, realFiles, Idioma.Español);
        Assert("Parse ES acepta key EN de respaldo", fbResult.Count == 1 && fbResult[0].Category == "Beatles");

        var dupe = ResponseParser.Parse("""{"categorias":{"A":["c1.mkv"],"B":["c1.mkv"]}}""", new[] { "c1.mkv" }, Idioma.Español);
        Assert("Parse conserva archivos en varias categorías", dupe.Count == 2 && dupe.Sum(r => r.Files.Count) == 2);

        var trash = ResponseParser.Parse("no hay json aquí", realFiles, Idioma.Español);
        Assert("Parse sin JSON devuelve vacío", trash.Count == 0);
        var empty = ResponseParser.Parse("""{"categorias":{"X":[]}}""", realFiles, Idioma.Español);
        Assert("Parse categoría vacía se descarta", empty.Count == 0);
        var missingFiles = ResponseParser.Parse("""{"categorias":{"A":["no-existe.txt"],"B":["c1.mkv"]}}""", new[] { "c1.mkv" }, Idioma.Español);
        Assert("Parse descarta archivos inexistentes", missingFiles.Count == 1 && missingFiles[0].Files.Count == 1);

        var sinCat = ResponseParser.Parse("""{"categorias":{"A":["../fuera.txt"]}}""", new[] { "../fuera.txt" }, Idioma.Español);
        Assert("Parse saneado de categoría", sinCat[0].Category == "A");
        Assert("Parse null response", ResponseParser.Parse(null!, new[] { "a.txt" }, Idioma.Español).Count == 0);
    }

    // ── Organizador ────────────────────────────────────────────────────

    private static void RunFileOrganizer()
    {
        string root = Path.Combine(Path.GetTempPath(), "clasificador-selftest-" + Guid.NewGuid().ToString("N"));
        string src = Path.Combine(root, "src");
        string dest = Path.Combine(root, "dest");
        Directory.CreateDirectory(src);
        File.WriteAllText(Path.Combine(src, "a.txt"), "a");
        File.WriteAllText(Path.Combine(src, "b.log"), "b");

        var results = new List<ClassificationResult>
        {
            new("Rock/Topic", new[] { "a.txt" }),
            new("Otra", new[] { "b.log" }),
            new("Vacía", Array.Empty<string>())
        };

        var copyResult = FileOrganizer.Organize(src, dest, results, copy: true, CancellationToken.None);
        Assert("Organizar copia 2", copyResult.Processed == 2 && copyResult.Errors == 0, $"{copyResult.Processed}/{copyResult.Errors}");
        Assert("Copia genera subcarpetas saneadas", File.Exists(Path.Combine(dest, "Rock_Topic", "a.txt")));
        Assert("Copia conserva origen", File.Exists(Path.Combine(src, "a.txt")));

        var moveResult = FileOrganizer.Organize(src, dest, new[] { new ClassificationResult("Movidos", new[] { "a.txt" }) }, copy: false, CancellationToken.None);
        Assert("Mover procesa", moveResult.Processed == 1);
        Assert("Mover borra origen", !File.Exists(Path.Combine(src, "a.txt")));

        int reports = 0;
        FileOrganizer.Organize(src, dest, new[] { new ClassificationResult("X", new[] { "b.log" }) }, true, CancellationToken.None, (_, _) => reports++);
        Assert("onProgress se invoca", reports > 0);

        Assert("Saneate carpeta null", FileOrganizer.SaneateFolderName(null!) == "SinCategoria");
        Assert("Saneate carácteres inválidos", FileOrganizer.SaneateFolderName("a<b>:c") == "a_b__c");
        Assert("Saneate longitud", FileOrganizer.SaneateFolderName(new string('x', 60)).Length == 50);

        try { Directory.Delete(root, true); } catch { }
    }

    // ── Filtros ────────────────────────────────────────────────────────

    private static void RunFileFilters()
    {
        Assert("Filtro .exe", FileFilters.IsSystemFile("app.exe"));
        Assert("Filtro .csproj", FileFilters.IsSystemFile("app.csproj"));
        Assert("Filtro .ps1", FileFilters.IsSystemFile("setup.ps1"));
        Assert("Filtro NO sistema", !FileFilters.IsSystemFile("foto.jpg"));
        Assert("Filtro NO sistema mp3", !FileFilters.IsSystemFile("cancion.mp3"));
        Assert("Filtro case-insensitive", FileFilters.IsSystemFile("LIB.DLL"));
    }

    // ── Traducciones ───────────────────────────────────────────────────

    private static void RunTranslations()
    {
        Assert("Tr ES PromptCopied", Translations.Get("PromptCopied", Idioma.Español) == "Prompt copiado a tu portapapeles");
        Assert("Tr EN no cae en ES", Translations.Get("ActionButton", Idioma.Inglés) == "Organize");
        Assert("Tr Modo label EN", Translations.ModeLabel("Música", Idioma.Inglés) == "🎵 Music");
        Assert("Tr modos existen", ClassificationModes.All.Count == 5);
    }

    // ── Modos ──────────────────────────────────────────────────────────

    private static void RunClassificationModes()
    {
        var musica = ClassificationModes.Find("Música");
        Assert("Find Música", musica is { Criterios.Length: 4 });
        Assert("Traducción criterio EN", ClassificationModes.TranslateCriterion("Género", Idioma.Inglés) == "Genre");
        Assert("Traducción criterio ES", ClassificationModes.TranslateCriterion("Tema", Idioma.Español) == "Tema");
        Assert("Reverse Gerne→Género", ClassificationModes.GetCriterionKey("Genre", Idioma.Inglés) == "Género");
        Assert("Find inexistente", ClassificationModes.Find("nope") is null);
        Assert("Criterios traducidos", ClassificationModes.GetCriteria(musica!, Idioma.Inglés)[0] == "Genre");
    }

    // ── BYOK store ────────────────────────────────────────────────────

    private static void RunByokConfigStore()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), "clasificador-byok-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var store = new ByokConfigStore(tempPath);

            var fresh = store.Load();
            Assert("Store default: 7 presets", fresh.Providers.Count == 7, $"got {fresh.Providers.Count}");
            Assert("Store default: activo openai", fresh.ActiveProviderId == "openai");
            Assert("Store default: ollama sin key requerida", !fresh.Providers.First(p => p.Id == "ollama").RequiresApiKey);

            fresh.Providers.Add(new AiProvider { Id = "custom-1", Name = "Mi API", BaseUrl = "https://x.example/v1", Models = { "m1" } });
            fresh.ActiveProviderId = "custom-1";
            store.Save(fresh);

            var reloaded = new ByokConfigStore(tempPath).Load();
            Assert("Store roundtrip: 8 providers", reloaded.Providers.Count == 8, $"got {reloaded.Providers.Count}");
            Assert("Store roundtrip: custom persistido", reloaded.ActiveProviderId == "custom-1");
            Assert("Store roundtrip: activo resuelto", reloaded.ActiveProvider?.Name == "Mi API");

            File.WriteAllText(tempPath, "{no-json}");
            var corrupt = new ByokConfigStore(tempPath).Load();
            Assert("Store corrupto: presets", corrupt.Providers.Count == 7);
        }
        finally
        {
            try { File.Delete(tempPath); } catch { }
        }
    }

    // ── AiClient offline ───────────────────────────────────────────────

    private static void RunAiClientOffline()
    {
        var client = new AiClient();
        var presets = AiProvider.DefaultPresets();
        var openai = presets.First(p => p.Id == "openai");
        var anthropic = presets.First(p => p.Id == "claude");
        var gemini = presets.First(p => p.Id == "gemini");
        var ollama = presets.First(p => p.Id == "ollama");
        openai.SelectedModel = "gpt-4o-mini";
        anthropic.SelectedModel = "claude-sonnet-4-5";
        gemini.SelectedModel = "gemini-2.5-flash";
        ollama.SelectedModel = "llama3.2";
        openai.ApiKey = "sk-test";
        anthropic.ApiKey = "ak-test";
        gemini.ApiKey = "gk-test";

        // Payloads
        var openAiPayload = client.BuildPayload(openai, "hola");
        Assert("Payload openai: response_format", openAiPayload.Contains("\"response_format\"") && openAiPayload.Contains("json_object"));
        Assert("Payload openai: modelo", openAiPayload.Contains("gpt-4o-mini"));

        var anthropicPayload = client.BuildPayload(anthropic, "hola");
        Assert("Payload anthropic: max_tokens", anthropicPayload.Contains("max_tokens"));
        Assert("Payload anthropic: sin response_format", !anthropicPayload.Contains("response_format"));

        var geminiPayload = client.BuildPayload(gemini, "hola");
        Assert("Payload gemini: responseMimeType", geminiPayload.Contains("responseMimeType") && geminiPayload.Contains("application/json"));

        var ollamaPayload = client.BuildPayload(ollama, "hola");
        Assert("Payload ollama: sin response_format (JsonMode=false)", !ollamaPayload.Contains("response_format"));

        // Headers / URLs
        var anthropicReq = client.BuildChatRequest(anthropic, "claude-sonnet-4-5", "hola");
        Assert("Header anthropic x-api-key", anthropicReq.Headers.Contains("x-api-key") && anthropicReq.Headers.GetValues("x-api-key").First() == "ak-test");
        Assert("Header anthropic version", anthropicReq.Headers.Contains("anthropic-version"));

        var geminiReq = client.BuildChatRequest(gemini, "gemini-2.5-flash", "hola");
        Assert("Header gemini x-goog-api-key", geminiReq.Headers.Contains("x-goog-api-key"));
        Assert("URL gemini :generateContent", geminiReq.RequestUri!.AbsoluteUri.Contains(":generateContent"));

        var openAiReq = client.BuildChatRequest(openai, "gpt-4o-mini", "hola");
        Assert("Header openai Bearer", openAiReq.Headers.GetValues("Authorization").First() == "Bearer sk-test");

        Assert("URL ollama tags", client.BuildListModelsUrl(ollama).AbsoluteUri == "http://localhost:11434/api/tags");
        Assert("URL openai models", client.BuildListModelsUrl(openai).AbsoluteUri == "https://api.openai.com/v1/models");
        Assert("URL gemini models", client.BuildListModelsUrl(gemini).AbsoluteUri == "https://generativelanguage.googleapis.com/v1beta/models");

        // Extracción
        string openAiText = client.ExtractResponseText(openai, """{"choices":[{"message":{"content":"{\"categorias\":{}}"}}]}""");
        Assert("Extract openai text", openAiText == """{"categorias":{}}""");

        string anthropicText = client.ExtractResponseText(anthropic, """{"content":[{"type":"text","text":"hola claude"}]}""");
        Assert("Extract anthropic text", anthropicText == "hola claude");

        string geminiText = client.ExtractResponseText(gemini, """{"candidates":[{"content":{"parts":[{"text":"hola gemini"}]}}]}""");
        Assert("Extract gemini text", geminiText == "hola gemini");

        Assert("Extract error gemini", ThrowsAi(gemini, """{"error":{"code":429,"message":"Quota","status":"RESOURCE_EXHAUSTED"}}""", 429, "RESOURCE_EXHAUSTED"));
        Assert("Extract error openai", ThrowsAi(openai, """{"error":{"message":"Incorrect key","code":"invalid_api_key"}}""", 0, "invalid_api_key"));
        Assert("Extract error anthropic", ThrowsAi(anthropic, """{"type":"error","error":{"type":"authentication_error","message":"invalid key","status_code":401}}""", 401, "authentication_error"));

        // Modelos
        Assert("Modelo usa SelectedModel", client.GetSelectedModel(openai) == "gpt-4o-mini");
        openai.SelectedModel = "";
        Assert("Modelo fallback primer preset", client.GetSelectedModel(openai) == "gpt-4o-mini");
        var vacio = new AiProvider { BaseUrl = "https://x.example/v1" };
        Assert("Modelo ausente lanza no_model", Throws(() => client.GetSelectedModel(vacio), 0, "no_model"));

        // Errores tempranos
        var deepseek = presets.First(p => p.Id == "deepseek");
        Assert("Key requerida openai", Throws(() => client.BuildChatRequest(deepseek, "m", "p"), 0, "missing_key"));
    }

    private static bool ThrowsAi(AiProvider p, string json, int statusCode, string errorCode)
    {
        try
        {
            new AiClient().ExtractResponseText(p, json);
            return false;
        }
        catch (AiException ex)
        {
            return ex.StatusCode == statusCode && ex.ErrorCode == errorCode;
        }
    }

    private static bool Throws(Func<object> action, int statusCode, string errorCode)
    {
        try
        {
            action();
            return false;
        }
        catch (AiException ex)
        {
            return ex.StatusCode == statusCode && ex.ErrorCode == errorCode;
        }
    }

    // ── DP FileListCustomContent (STA) ─────────────────────────────────

    private static void RunFileListCustomContent()
    {
        var control = new BaseMainControl();
        var panel = (ContentControl)control.FindName("FileListCustomPanel")!;
        var fileList = control.FileListBox;

        Assert("DP inicial: panel oculto", panel.Visibility == Visibility.Collapsed);
        Assert("DP inicial: lista visible", fileList.Visibility == Visibility.Visible);

        var content = new TextBlock { Text = "tree" };
        control.FileListCustomContent = content;
        Assert("DP set: panel visible", panel.Visibility == Visibility.Visible);
        Assert("DP set: contenido asignado", ReferenceEquals(panel.Content, content));
        Assert("DP set: lista colapsada", fileList.Visibility == Visibility.Collapsed);

        control.FileListCustomContent = null;
        Assert("DP null: panel oculto otra vez", panel.Visibility == Visibility.Collapsed);
        Assert("DP null: lista visible otra vez", fileList.Visibility == Visibility.Visible);
        Assert("DP null: contenido limpio", panel.Content == null);
    }
}