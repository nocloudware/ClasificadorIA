using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ClasificadorIA.Models;
using ClasificadorIA.Panels;
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
        RunLocalClassifier();
        RunMetadataClassifier();
        RunBatchClassifier();
        RunFileOrganizer();
        RunFileFilters();
        RunTranslations();
        RunClassificationModes();
        RunByokConfigStore();
        RunByokDialogLabels();
        RunAiClientOffline();
        RunDedupFilter();
        RunFileListCustomContent();
        RunEndToEnd();

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

        string es = PromptGenerator.AssignmentBatch(mode, "Género", Idioma.Español, files);
        Assert("Lote ES contiene criterio", es.Contains("género"));
        Assert("Lote ES lista archivos", es.Contains("- cancion1.mp3"));
        Assert("Lote ES pide formato archivos", es.Contains("\"archivos\""));

        string en = PromptGenerator.AssignmentBatch(mode, "Género", Idioma.Inglés, files);
        Assert("Lote EN contiene criterio", en.Contains("genre"));
        Assert("Lote EN usa descripcion EN", en.Contains("classifying songs"));
        Assert("Lote EN pide formato files", en.Contains("\"files\""));

        var stats = new[] { new CategoryStat("Rock", 12, new[] { "a.mp3", "b.mp3" }), new CategoryStat("Pop", 8, new[] { "c.mp3" }) };
        string consEs = PromptGenerator.Consolidate(stats, 5, Idioma.Español);
        Assert("Consolidación ES incluye categoría y conteo", consEs.Contains("Rock (12"));
        Assert("Consolidación ES pide formato finales", consEs.Contains("\"finales\""));
        string consEn = PromptGenerator.Consolidate(stats, 5, Idioma.Inglés);
        Assert("Consolidación EN pide formato final", consEn.Contains("\"final\""));
        Assert("Consolidación vacía", PromptGenerator.Consolidate(Array.Empty<CategoryStat>(), 5, Idioma.Español) == "");

        string de = PromptGenerator.AssignmentBatch(mode, "Género", Idioma.Alemán, files);
        Assert("Lote DE usa formato dateien", de.Contains("\"dateien\""));
        Assert("Lote DE no usa template EN", !de.Contains("classifying songs"));

        string consDe = PromptGenerator.Consolidate(stats, 5, Idioma.Alemán);
        Assert("Consolidación DE usa final", consDe.Contains("\"final\""));
    }

    // ── Parser ─────────────────────────────────────────────────────────

    private static void RunResponseParser()
    {
        var realFiles = new[] { "album1.mp3", "Red Hot Chili Peppers - Under The Bridge.mp3", "Bad Bunny - X.mp3", "The Beatles - Hey Jude.mp3" };

        string es = """{"archivos": [{"archivo": "album1.MP3", "categoria": "Rock"}, {"archivo": "bad bunny - x.mp3", "categoria": "Reggaetón"}, {"archivo": "fantasma.mp3", "categoria": "X"}]}""";
        var esAssign = ResponseParser.ParseBatchAssignments(es, realFiles, Idioma.Español);
        Assert("Asignación ES matchea case-insensitive", esAssign.TryGetValue("album1.mp3", out var c1) && c1 == "Rock");
        Assert("Asignación ES ignora archivos inexistentes", !esAssign.ContainsKey("fantasma.mp3"));
        Assert("Asignación ES cuenta", esAssign.Count == 2, $"got {esAssign.Count}");

        string en = "```json\n{\"files\": [{\"file\": \"Red Hot Chili Peppers - Under The Bridge.mp3\", \"category\": \"Rock\"}]}\n```";
        var enAssign = ResponseParser.ParseBatchAssignments(en, realFiles, Idioma.Inglés);
        Assert("Asignación EN con código markdown", enAssign.Count == 1 && enAssign["Red Hot Chili Peppers - Under The Bridge.mp3"] == "Rock");

        string fallback = """{"files": [{"file": "album1.mp3", "category": "Beatles"}]}""";
        var fbAssign = ResponseParser.ParseBatchAssignments(fallback, realFiles, Idioma.Español);
        Assert("Asignación ES acepta key EN de respaldo", fbAssign.Count == 1 && fbAssign["album1.mp3"] == "Beatles");

        Assert("Asignación sin JSON devuelve vacío", ResponseParser.ParseBatchAssignments("no hay json aquí", realFiles, Idioma.Español).Count == 0);
        Assert("Asignación null devuelve vacío", ResponseParser.ParseBatchAssignments(null!, realFiles, Idioma.Español).Count == 0);

        string mapEs = """{"finales": {"Rock": ["rock", "rock clásico"], "Reggaetón": ["reggeaton"]}}""";
        var esMap = ResponseParser.ParseConsolidationMap(mapEs, Idioma.Español);
        Assert("Mapa ES parsea", esMap != null && esMap.Count == 2, esMap == null ? "null" : $"{esMap.Count}");
        Assert("Mapa ES agrupa fuentes", esMap!["Rock"].Length == 2);

        string mapEn = """{"final": {"Pop": ["pop", "pop rock"]}}""";
        var enMap = ResponseParser.ParseConsolidationMap(mapEn, Idioma.Inglés);
        Assert("Mapa EN parsea", enMap != null && enMap["Pop"].Length == 2);

        string mapFallback = """{"final": {"Una": ["otra"]}}""";
        var fbMap = ResponseParser.ParseConsolidationMap(mapFallback, Idioma.Español);
        Assert("Mapa ES acepta key EN de respaldo", fbMap != null && fbMap["Una"].Length == 1);

        Assert("Mapa sin JSON devuelve null", ResponseParser.ParseConsolidationMap("nada", Idioma.Español) == null);
        Assert("Mapa null devuelve null", ResponseParser.ParseConsolidationMap(null!, Idioma.Español) == null);

        string de = """{"dateien": [{"datei": "album1.mp3", "kategorie": "Rock"}]}""";
        var deAssign = ResponseParser.ParseBatchAssignments(de, realFiles, Idioma.Alemán);
        Assert("Asignación DE con claves alemanas", deAssign.Count == 1 && deAssign["album1.mp3"] == "Rock");

        string deMap = """{"final": {"Rock": ["rock"]}}""";
        var deMapRes = ResponseParser.ParseConsolidationMap(deMap, Idioma.Alemán);
        Assert("Mapa DE parsea key final", deMapRes != null && deMapRes["Rock"].Length == 1);
    }

    // ── Clasificador local ─────────────────────────────────────────────

    private static void RunLocalClassifier()
    {
        // Genérico + Tema: agrupa por token compartido; el que no calza va a Otros.
        var beatles = new[] { "The Beatles - Hey Jude.mp3", "The Beatles - Let It Be.mp3", "Queen - Under Pressure.mp3" };
        var batles = LocalClassifier.Classify(beatles, 5, Idioma.Español);
        Assert("Local Tema: grupo por token compartido", batles.Any(r => r.Category == "Beatles" && r.Files.Count == 2), string.Join(",", batles.Select(b => $"{b.Category}({b.Files.Count})")));
        Assert("Local Tema: sin tema → Otros", batles.Any(r => r.Category == "Otros" && r.Files.Count == 1));

        // Tema por tokens con y sin tema.
        var mixed = new[] { "vacaciones montaña.jpg", "vacaciones playa.jpg", "capitulo1.pdf" };
        var mixto = LocalClassifier.Classify(mixed, 5, Idioma.Español);
        Assert("Local Tema: tema sobre archivo suelto (vacaciones)", mixto.Any(r => r.Category == "Vacaciones" && r.Files.Count == 2), string.Join(",", mixto.Select(b => $"{b.Category}({b.Files.Count})")));
        Assert("Local Tema: pdf sin tema → Otros (sin extensión)", mixto.Any(r => r.Category == "Otros" && r.Files.Count == 1));

        // Stopwords no forman tema.
        var stop = new[] { "de el la.mp3", "de el la y.mp3" };
        var noTema = LocalClassifier.Classify(stop, 5, Idioma.Español);
        Assert("Local Tema: stopwords → Otros", noTema.All(r => r.Category == "Otros"));

        // Sufijo de serie en casi todos los archivos no colapsa todo en una categoría.
        var serie = new[] { "Marte, el planeta rojo ｜ Ciencia Para Dormir.m4a", "La Luna y su origen ｜ Ciencia Para Dormir.m4a", "Neptuno, gigante helado ｜ Ciencia Para Dormir.m4a", "La Luna vista de cerca ｜ Ciencia Para Dormir.m4a" };
        var serieRes = LocalClassifier.Classify(serie, 5, Idioma.Español);
        Assert("Local Tema: sufijo de serie no crea categoría gigante", !serieRes.Any(r => r.Category == "Ciencia" || r.Category == "Dormir"), string.Join(",", serieRes.Select(b => $"{b.Category}({b.Files.Count})")));
        Assert("Local Tema: subtema real sobre sufijo de serie (luna)", serieRes.Any(r => r.Category == "Luna" && r.Files.Count == 2), string.Join(",", serieRes.Select(b => $"{b.Category}({b.Files.Count})")));

        // Recorte por profundidad.
        var many = new[] { "rock-a.mp3", "rock-b.mp3", "pop-a.mp3", "pop-b.mp3", "jazz-a.mp3", "jazz-b.mp3", "folk-a.mp3", "folk-b.mp3", "solo-x.mp3" };
        var recortado = LocalClassifier.Classify(many, 3, Idioma.Español);
        Assert("Local: recorte por profundidad", recortado.Count <= 3, $"count {recortado.Count}");
        Assert("Local: recorte conserva las mayores", recortado.Any(r => r.Category == "Rock" && r.Files.Count == 2));
        Assert("Local: recorte fusiona el resto en Otros", recortado.Any(r => r.Category == "Otros"));

        Assert("Local: sin archivos devuelve vacío", LocalClassifier.Classify(Array.Empty<string>(), 5, Idioma.Español).Count == 0);
        Assert("CapToDepth bajo límite no toca", LocalClassifier.CapToDepth(new[] { new ClassificationResult("A", new[] { "x" }) }, 5, "Otros").Count == 1);

        // CapToDepth: el sobrante se suma a un "Otros" existente, no crea un duplicado.
        var conOtros = new List<ClassificationResult>
        {
            new("Pop", new[] { "p1", "p2", "p3" }),
            new("Otros", new[] { "o1", "o2" }),
            new("Rock", new[] { "r1" }),
            new("Jazz", new[] { "j1", "j2" })
        };
        var capOtros = LocalClassifier.CapToDepth(conOtros, 3, "Otros");
        Assert("Cap: no duplica Otros", capOtros.Count(r => r.Category == "Otros") == 1, string.Join(",", capOtros.Select(b => $"{b.Category}({b.Files.Count})")));
        Assert("Cap: sobrante se suma al Otros existente", capOtros.First(r => r.Category == "Otros").Files.Count == 5, string.Join(",", capOtros.Select(b => $"{b.Category}({b.Files.Count})")));

        // Música + Género: sin etiqueta de género → Otros (ni extensión, ni nombres ni artistas).
        var sinTags = new[] { @"C:\x\Soda Stereo - De Música Ligera.mp3", @"C:\x\Aerosmith - Walk This Way.mp3" };
        var genero = LocalClassifier.Classify(sinTags, 5, Idioma.Español, "Género");
        Assert("Local Género: sin etiqueta → Otros", genero.Any(r => r.Category == "Otros" && r.Files.Count == 2), string.Join(",", genero.Select(b => $"{b.Category}({b.Files.Count})")));
        Assert("Local Género: no aparecen artistas ni extensiones", !genero.Any(r => r.Category == "Soda" || r.Category == "Audio" || r.Category == "Aerosmith"));

        // Otro criterio sin metadatos disponibles también va a Otros.
        var docs = new[] { "cap1.pdf", "cap2.pdf" };
        var direc = LocalClassifier.Classify(docs, 5, Idioma.Español, "Director");
        Assert("Local Director: sin fuente → Otros", direc.All(r => r.Category == "Otros"));
    }

    // ── Micro-clasificador por metadatos ──────────────────────────────

    private static void RunMetadataClassifier()
    {
        string root = Path.Combine(Path.GetTempPath(), "metaclass-selftest-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            // Video con patrón de serie: sin criterio (Tema) → null, ya no arma "Temporada".
            string serie = Path.Combine(root, "S01E05.mp4");
            File.WriteAllText(serie, "x");
            Assert("Meta: video solo temporada no calza en Tema", MetadataClassifier.TryClassify(serie, null, Idioma.Español) == null);

            // Video → año con criterio Año; sin género → null.
            string video = Path.Combine(root, "vacaciones.mp4");
            File.WriteAllText(video, "x");
            int year = File.GetLastWriteTime(video).Year;
            Assert("Meta: video Año → año", MetadataClassifier.TryClassify(video, "Año", Idioma.Español) == year.ToString());
            Assert("Meta: video no tiene Género", MetadataClassifier.TryClassify(video, "Género", Idioma.Español) == null);

            // Imagen → año (sin EXIF usa fecha del archivo).
            string img = Path.Combine(root, "foto.jpg");
            File.WriteAllText(img, "x");
            Assert("Meta: imagen Año → año", MetadataClassifier.TryClassify(img, "Año", Idioma.Español) == year.ToString());

            // Documento → año.
            string doc = Path.Combine(root, "nota.txt");
            File.WriteAllText(doc, "x");
            Assert("Meta: doc → año", MetadataClassifier.TryClassify(doc, "Año", Idioma.Español) == year.ToString());

            // Audio sin tags válidos (no es audio real) → null → cae a Otros.
            string fake = Path.Combine(root, "cancion.mp3");
            File.WriteAllText(fake, "no-es-audio");
            Assert("Meta: audio corrupto → null", MetadataClassifier.TryClassify(fake, "Género", Idioma.Español) == null);

            // Celdas por modo: una por criterio, alineadas con GetCriteria (encabezados), vacías sin fuente.
            var cellsMusica = MetadataClassifier.GetCells(video, ClassificationModes.Find("Música")!, Idioma.Español);
            Assert("Cells Música: 4 celdas", cellsMusica.Length == 4, $"got {cellsMusica.Length}");
            Assert("Cells Música: video sin tags → todas vacías", cellsMusica.All(c => c == ""), string.Join(",", cellsMusica));

            var cellsPeliculas = MetadataClassifier.GetCells(video, ClassificationModes.Find("Películas")!, Idioma.Español);
            Assert("Cells Películas: 4 celdas", cellsPeliculas.Length == 4, $"got {cellsPeliculas.Length}");
            Assert("Cells Películas: Año en slot 2", cellsPeliculas[2] == year.ToString(), string.Join(",", cellsPeliculas));
            Assert("Cells Películas: Género sin fuente vacío", cellsPeliculas[0] == "");

            var cellsGenerico = MetadataClassifier.GetCells(video, ClassificationModes.Default, Idioma.Español);
            Assert("Cells Genérico: 1 celda", cellsGenerico.Length == 1, $"got {cellsGenerico.Length}");
            Assert("Cells Genérico: Tema sin fuente → vacía", cellsGenerico[0] == "");
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }

        // Normalización de géneros.
        Assert("Genre: other → Otros", MetadataClassifier.NormalizeGenre("other", Idioma.Español) == "Otros");
        Assert("Genre: compuesto → primer género", MetadataClassifier.NormalizeGenre("Electronic - Pop - New Wave - Synth", Idioma.Español) == "Electronic");
        Assert("Genre: pop, rock → pop", MetadataClassifier.NormalizeGenre("Pop, Rock", Idioma.Español) == "Pop");
        Assert("Genre: minúscula → título", MetadataClassifier.NormalizeGenre("pop", Idioma.Español) == "Pop");
        Assert("Genre: r&b / rhythm and blues", MetadataClassifier.NormalizeGenre("Rhythm and Blues", Idioma.Español) == "R&B");
        Assert("Genre: synthpop → new wave", MetadataClassifier.NormalizeGenre("Synthpop", Idioma.Español) == "New Wave");
        Assert("Genre: films → banda sonora", MetadataClassifier.NormalizeGenre("Films", Idioma.Español) == "Banda sonora");
        Assert("Genre: espacio a secas → null", MetadataClassifier.NormalizeGenre("  ", Idioma.Español) == null);
    }

    // ── Clasificador por lotes (fake client) ──────────────────────────

    private static void RunBatchClassifier()
    {
        // Fase 1 devuelve categorías por archivo; fase 2 consolida.
        var files = Enumerable.Range(1, 5).Select(i => $"can{i}.mp3").ToArray();
        var classifier = new BatchClassifier((prompt, _) =>
            prompt.Contains("\"archivos\"")
                ? Task.FromResult("""{"archivos":[{"archivo":"can1.mp3","categoria":"rock"},{"archivo":"can2.mp3","categoria":"rock"},{"archivo":"can3.mp3","categoria":"pop"},{"archivo":"can4.mp3","categoria":"pop"},{"archivo":"can5.mp3","categoria":"indie"}]}""")
                : Task.FromResult("""{"finales":{"Rock":["rock"],"Pop":["pop"]}}"""),
            batchSize: 2, depth: 10);

        var results = classifier.ClassifyAsync(ClassificationModes.Find("Música")!, "Género", Idioma.Español, files).GetAwaiter().GetResult();
        Assert("Batch: chunks producen resultado", results.Sum(r => r.Files.Count) == 5, string.Join(",", results.Select(r => $"{r.Category}({r.Files.Count})")));
        Assert("Batch: consolidación mapeada", results.Any(r => r.Category == "Rock" && r.Files.Count == 2));
        Assert("Batch: categoría sin mapear va a Otros", results.Any(r => r.Category == "Otros" && r.Files.Count == 1));

        // Cuota (rate-limit): reintenta con la espera que pide el proveedor y termina bien.
        int attempts = 0;
        var retryClassifier = new BatchClassifier((prompt, _) =>
        {
            attempts++;
            if (attempts < 3) return Task.FromException<string>(new AiException(429, "RESOURCE_EXHAUSTED", "Please retry in 0s."));
            return Task.FromResult("""{"archivos":[{"archivo":"can1.mp3","categoria":"rock"},{"archivo":"can2.mp3","categoria":"pop"}]}""");
        }, batchSize: 2, depth: 10);
        var retryResult = retryClassifier.ClassifyAsync(ClassificationModes.Default, "Tema", Idioma.Español, new[] { "can1.mp3", "can2.mp3" }).GetAwaiter().GetResult();
        Assert("Batch: cuota reintenta hasta resolver", retryResult.Sum(r => r.Files.Count) == 2, string.Join(",", retryResult.Select(r => $"{r.Category}({r.Files.Count})")));

        // onBatchStarted anuncia cada lote (1..N) con el conteo correcto.
        var starts = new List<(int Index, int Count)>();
        var startClassifier = new BatchClassifier((prompt, _) =>
            prompt.Contains("\"archivos\"")
                ? Task.FromResult("""{"archivos":[{"archivo":"can1.mp3","categoria":"a"},{"archivo":"can2.mp3","categoria":"b"},{"archivo":"can3.mp3","categoria":"c"}]}""")
                : Task.FromResult("""{"finales":{"A":["a"],"B":["b"]}}"""),
            batchSize: 2, depth: 10);
        startClassifier.ClassifyAsync(ClassificationModes.Default, "Tema", Idioma.Español, new[] { "can1.mp3", "can2.mp3", "can3.mp3" },
            onBatchStarted: (i, n) => starts.Add((i, n))).GetAwaiter().GetResult();
        Assert("Batch: onBatchStarted por lote", starts.Count == 2 && starts[0] == (1, 2) && starts[1] == (2, 2), string.Join(",", starts));

        // Cancelación: token cancelado aborta sin llamar al generador.
        int cancelledCalls = 0;
        var cancelClassifier = new BatchClassifier((prompt, _) =>
        {
            cancelledCalls++;
            return Task.FromResult("{}");
        }, batchSize: 2, depth: 10);
        bool aborted = false;
        try
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            cancelClassifier.ClassifyAsync(ClassificationModes.Default, "Tema", Idioma.Español, files, ct: cts.Token).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            aborted = true;
        }
        Assert("Batch: cancelación aborta sin generar", aborted && cancelledCalls == 0, $"aborted={aborted}, calls={cancelledCalls}");

        // Error que no es de cuota → se propaga (no se miente con "Otros").
        int noRetryCalls = 0;
        var hardFailClassifier = new BatchClassifier((_, _) =>
        {
            noRetryCalls++;
            return Task.FromException<string>(new AiException(0, "network", "corte"));
        }, batchSize: 2, depth: 10);
        bool propagado = false;
        try
        {
            hardFailClassifier.ClassifyAsync(ClassificationModes.Default, "Tema", Idioma.Español, files).GetAwaiter().GetResult();
        }
        catch (AiException)
        {
            propagado = true;
        }
        Assert("Batch: error no-cuota se propaga", propagado && noRetryCalls == 1, $"calls={noRetryCalls}, propagado={propagado}");

        // "Todos": batchSize enorme → un único lote.
        int allCalls = 0;
        var allClassifier = new BatchClassifier((prompt, _) =>
        {
            allCalls++;
            return prompt.Contains("\"archivos\"")
                ? Task.FromResult("""{"archivos":[{"archivo":"can1.mp3","categoria":"rock"},{"archivo":"can2.mp3","categoria":"rock"}]}""")
                : Task.FromResult("""{"finales":{"Rock":["rock"]}}""");
        }, batchSize: int.MaxValue, depth: 10);
        var allResult = allClassifier.ClassifyAsync(ClassificationModes.Default, "Tema", Idioma.Español, new[] { "can1.mp3", "can2.mp3" }).GetAwaiter().GetResult();
        Assert("Batch: Todos → una sola llamada de asignación", allCalls == 2, $"calls={allCalls}");

        // Límite del modelo: lote "Todos" se achica para que quepa (deepseek-chat: 8192 → (8192-2000)/30 = 206 archivos/lote).
        int shrunkCalls = 0;
        var shrinkClassifier = new BatchClassifier((prompt, _) =>
        {
            shrunkCalls++;
            return prompt.Contains("\"archivos\"")
                ? Task.FromResult("""{"archivos":[{"archivo":"223.mp3","categoria":"rock"}]}""")
                : Task.FromResult("""{"finales":{"Rock":["rock"]}}""");
        }, batchSize: int.MaxValue, depth: 10, maxOutputLimit: 8192);
        var shrinkResult = shrinkClassifier.ClassifyAsync(ClassificationModes.Default, "Tema", Idioma.Español,
            Enumerable.Range(1, 500).Select(i => $"{i}.mp3").ToArray()).GetAwaiter().GetResult();
        Assert("Batch: lote achicado por límite del modelo", shrunkCalls == 4, $"calls={shrunkCalls} (esperado 3 asignaciones + 1 consolidación)");

        // Incremental: el lote 2 recibe las categorías del lote 1 como guía.
        var assignmentPrompts = new List<string>();
        var incrementalClassifier = new BatchClassifier((prompt, _) =>
        {
            string assignment = prompt.Contains("\"archivos\"")
                ? prompt.Contains("can1.mp3") ? "rock" : "pop"
                : "";
            if (prompt.Contains("\"archivos\""))
            {
                assignmentPrompts.Add(prompt);
                return Task.FromResult($$"""{"archivos":[{"archivo":"{{(prompt.Contains("can1.mp3") ? "can1.mp3" : "can2.mp3")}}","categoria":"{{assignment}}"}]}""");
            }
            return Task.FromResult("""{"finales":{"Rock":["rock"],"Pop":["pop"]}}""");
        }, batchSize: 1, depth: 10);
        incrementalClassifier.ClassifyAsync(ClassificationModes.Default, "Tema", Idioma.Español, new[] { "can1.mp3", "can2.mp3" }).GetAwaiter().GetResult();
        Assert("Batch: 2 lotes de asignación", assignmentPrompts.Count == 2, $"count={assignmentPrompts.Count}");
        Assert("Lote 1 no menciona categorías previas", !assignmentPrompts[0].Contains("rock"));
        Assert("Lote 2 incluye categoría del lote 1", assignmentPrompts[1].Contains("rock"), assignmentPrompts[1]);

        // Recorte por profundidad tras consolidación (3 finales, depth 2).
        var recorteClassifier = new BatchClassifier((prompt, _) =>
            prompt.Contains("\"archivos\"")
                ? Task.FromResult("""{"archivos":[{"archivo":"can1.mp3","categoria":"a"},{"archivo":"can2.mp3","categoria":"b"}]}""")
                : Task.FromResult("""{"finales":{"A":["a"],"B":["b"],"C":["a"]}}"""),
            batchSize: 2, depth: 2);
        var recorteResult = recorteClassifier.ClassifyAsync(ClassificationModes.Default, "Tema", Idioma.Español, new[] { "can1.mp3", "can2.mp3" }).GetAwaiter().GetResult();
        Assert("Batch: recorte por profundidad", recorteResult.Count <= 2, $"count {recorteResult.Count}");

        // Consolidación fallida → se conservan categorías crudas.
        var falloConsolidacion = new BatchClassifier((prompt, _) =>
            prompt.Contains("\"archivos\"")
                ? Task.FromResult("""{"archivos":[{"archivo":"can1.mp3","categoria":"rock"}]}""")
                : Task.FromException<string>(new AiException(0, "network", "corte en consolidación")),
            batchSize: 2, depth: 10);
        var rawResult = falloConsolidacion.ClassifyAsync(ClassificationModes.Default, "Tema", Idioma.Español, new[] { "can1.mp3" }).GetAwaiter().GetResult();
        Assert("Batch: consolidación fallida conserva crudas", rawResult.Any(r => r.Category == "rock" && r.Files.Count == 1), string.Join(",", rawResult.Select(r => $"{r.Category}({r.Files.Count})")));
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

        var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["a.txt"] = Path.Combine(src, "a.txt"),
            ["b.log"] = Path.Combine(src, "b.log")
        };

        var copyResult = FileOrganizer.Organize(paths, dest, results, copy: true, CancellationToken.None);
        Assert("Organizar copia 2", copyResult.Processed == 2 && copyResult.Errors == 0, $"{copyResult.Processed}/{copyResult.Errors}");
        Assert("Copia genera subcarpetas saneadas", File.Exists(Path.Combine(dest, "Rock_Topic", "a.txt")));
        Assert("Copia conserva origen", File.Exists(Path.Combine(src, "a.txt")));

        var moveResult = FileOrganizer.Organize(paths, dest, new[] { new ClassificationResult("Movidos", new[] { "a.txt" }) }, copy: false, CancellationToken.None);
        Assert("Mover procesa", moveResult.Processed == 1);
        Assert("Mover borra origen", !File.Exists(Path.Combine(src, "a.txt")));

        int reports = 0;
        FileOrganizer.Organize(paths, dest, new[] { new ClassificationResult("X", new[] { "b.log" }) }, true, CancellationToken.None, (_, _) => reports++);
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
        Assert("Tr ES MethodLocal", Translations.Get("MethodLocal", Idioma.Español) == "Local");
        Assert("Tr EN no cae en ES", Translations.Get("ActionButton", Idioma.Inglés) == "Organize");
        Assert("Tr Modo label EN", Translations.ModeLabel("Música", Idioma.Inglés) == "🎵 Music");
        Assert("Tr modos existen", ClassificationModes.All.Count == 5);
        Assert("Tr Otros EN", Translations.Get("Otros", Idioma.Inglés) == "Others");
        Assert("Tr BatchSize ES", Translations.Get("BatchSize", Idioma.Español) == "Tamaño de lote");

        string assignFr = PromptGenerator.AssignmentBatch(ClassificationModes.Find("Música")!, "Género", Idioma.Francés, new[] { "a.mp3" });
        Assert("Prompt 6 idiomas definidos", new[]
        {
            PromptGenerator.AssignmentBatch(ClassificationModes.Default, "Tema", Idioma.Francés, new[] { "a.mp3" }),
            assignFr,
            PromptGenerator.AssignmentBatch(ClassificationModes.Default, "Tema", Idioma.Alemán, new[] { "a.mp3" }),
            PromptGenerator.AssignmentBatch(ClassificationModes.Default, "Tema", Idioma.Portugués, new[] { "a.mp3" }),
            PromptGenerator.AssignmentBatch(ClassificationModes.Default, "Tema", Idioma.Italiano, new[] { "a.mp3" }),
            PromptGenerator.AssignmentBatch(ClassificationModes.Default, "Tema", Idioma.Japonés, new[] { "a.mp3" }),
            PromptGenerator.AssignmentBatch(ClassificationModes.Default, "Tema", Idioma.Chino, new[] { "a.mp3" }),
        }.All(p => !string.IsNullOrWhiteSpace(p) && !p.Contains("classifying files")));

        Assert("Prompt FR pide formato fichiers", assignFr.Contains("fichiers"));

        // Cada idioma nuevo traduce de verdad (si faltara la clave, caería a EN y el assert falla).
        var nuevos = new (Idioma Lang, string Otros, string ModoMusica)[]
        {
            (Idioma.Francés, "Autres", "🎵 Musique"),
            (Idioma.Alemán, "Andere", "🎵 Musik"),
            (Idioma.Portugués, "Outros", "🎵 Música"),
            (Idioma.Italiano, "Altri", "🎵 Musica"),
            (Idioma.Japonés, "その他", "🎵 音楽"),
            (Idioma.Chino, "其他", "🎵 音乐"),
        };
        foreach (var (lang, otros, modo) in nuevos)
        {
            Assert($"Tr {lang} Otros", Translations.Get("Otros", lang) == otros);
            Assert($"Tr {lang} Modo label", Translations.ModeLabel("Música", lang) == modo);
            Assert($"Tr {lang} ActionButton", Translations.Get("ActionButton", lang) != "Organize");
            Assert($"Tr {lang} UpdateAvailable", Translations.Get("UpdateAvailable", lang).Contains("{0}"));
            Assert($"Tr {lang} criterio", ClassificationModes.TranslateCriterion("Género", lang) == (lang == Idioma.Portugués ? "Gênero" : lang switch
            {
                Idioma.Francés => "Genre",
                Idioma.Alemán => "Genre",
                Idioma.Italiano => "Genere",
                Idioma.Japonés => "ジャンル",
                _ => "类型",
            }));
        }
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

            foreach (var preset in AiProvider.DefaultPresets())
            {
                bool uriOk = string.IsNullOrEmpty(preset.ApiKeyUrl)
                    || Uri.TryCreate(preset.ApiKeyUrl, UriKind.Absolute, out _);
                Assert($"Preset {preset.Id} ApiKeyUrl no crashea Uri", uriOk, preset.ApiKeyUrl);
            }

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

    private static void RunByokDialogLabels()
    {
        var dialog = new ByokDialog(new ByokConfig(), new ByokConfigStore(), new AiClient(), Idioma.Español);
        var labels = new (string Name, string Value)[]
        {
            ("Title", dialog.ByokDialogTitle),
            ("Subtitle", dialog.ByokDialogSubtitle),
            ("Provider", dialog.ProviderLabel),
            ("Scheme", dialog.SchemeLabel),
            ("BaseUrl", dialog.BaseUrlLabel),
            ("ApiKey", dialog.ApiKeyLabel),
            ("Model", dialog.ModelLabel),
            ("ModelHint", dialog.ModelHint),
            ("ReloadModels", dialog.ReloadModelsLabel),
            ("Temperature", dialog.TemperatureLabel),
            ("TemperatureHint", dialog.TemperatureHint),
            ("MaxTokens", dialog.MaxTokensLabel),
            ("MaxTokensHint", dialog.MaxTokensHint),
            ("Delete", dialog.DeleteLabel),
            ("Test", dialog.TestConnectionLabel),
            ("Save", dialog.SaveLabel),
            ("Cancel", dialog.CancelLabel)
        };
        foreach (var l in labels)
            Assert("ByokDialog label " + l.Name, !string.IsNullOrWhiteSpace(l.Value), l.Value);
    }

    private static void RunAiClientOffline()
    {
        var client = new AiClient();
        var presets = AiProvider.DefaultPresets();
        var openai = presets.First(p => p.Id == "openai");
        var anthropic = presets.First(p => p.Id == "claude");
        var gemini = presets.First(p => p.Id == "gemini");
        var ollama = presets.First(p => p.Id == "ollama");
        var deepseek = presets.First(p => p.Id == "deepseek");
        openai.SelectedModel = "gpt-4o-mini";
        anthropic.SelectedModel = "claude-sonnet-4-5";
        gemini.SelectedModel = "gemini-2.5-flash";
        ollama.SelectedModel = "llama3.2";
        openai.ApiKey = "sk-test";
        anthropic.ApiKey = "ak-test";
        gemini.ApiKey = "gk-test";
        deepseek.SelectedModel = "deepseek-v4-flash";

        // Payloads
        var openAiPayload = client.BuildPayload(openai, "hola");
        Assert("Payload openai: response_format", openAiPayload.Contains("\"response_format\"") && openAiPayload.Contains("json_object"));
        Assert("Payload openai: modelo", openAiPayload.Contains("gpt-4o-mini"));
        Assert("Payload openai: sin thinking", !openAiPayload.Contains("\"thinking\""));

        var deepseekPayload = client.BuildPayload(deepseek, "hola");
        Assert("Payload deepseek: thinking desactivado", deepseekPayload.Contains("\"thinking\":{\"type\":\"disabled\"}"));

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

        // Razonamiento (deepseek-v4-flash/reasoner): content vacío → se lee reasoning_content.
        string reasoning = client.ExtractResponseText(openai, """{"choices":[{"message":{"content":"","reasoning_content":"{\"archivos\":[]}"}}]}""");
        Assert("Extract openai reasoning fallback", reasoning == """{"archivos":[]}""");

        // Contenido vacío sin reasoning → error.
        Assert("Extract contenido vacío lanza", Throws(() => client.ExtractResponseText(openai, """{"choices":[{"message":{"content":""}}]}"""), 200, "bad_response"));

        // Presupuesto de tokens: fijo, y recorte por límite del modelo.
        openai.MaxTokens = 8192;
        Assert("Payload max_tokens fijo", client.BuildPayload(openai, "hola").Contains("\"max_tokens\":8192"));
        openai.MaxTokens = MaxTokensConst.Max;
        openai.MaxOutputLimit = 8000;
        Assert("Payload max_tokens recortado al límite del modelo", client.BuildPayload(openai, "hola").Contains("\"max_tokens\":8000"));
        openai.MaxOutputLimit = null;

        // Cálculo automático de presupuesto.
        Assert("Auto tokens piso Mini", MaxTokensConst.ForFiles(5) == MaxTokensConst.Mini, MaxTokensConst.ForFiles(5).ToString());
        Assert("Auto tokens intermedio", MaxTokensConst.ForFiles(100) == 5000);
        Assert("Auto tokens mil+ clampa al tope", MaxTokensConst.ForFiles(1091) == MaxTokensConst.Max);

        // Límites de modelo: estáticos y en vivo.
        Assert("Límite estático deepseek-v4", ModelLimits.LookupStatic("deepseek-v4-flash") == 384_000);
        Assert("Límite estático deepseek-chat", ModelLimits.LookupStatic("deepseek-chat") == 8192);
        Assert("Límite estático gpt-4o-mini", ModelLimits.LookupStatic("gpt-4o-mini") == 16384);
        Assert("Límite estático desconocido", ModelLimits.LookupStatic("modelo-raro-123") == null);
        var liveProvider = new AiProvider { ModelOutputLimits = { ["mi-modelo"] = 5000 } };
        Assert("Límite en vivo gana", ModelLimits.Resolve(liveProvider, "mi-modelo") == 5000);
        Assert("Límite desconocido no limita", ModelLimits.Resolve(liveProvider, "otro-modelo") == null);

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

    // ── E2E (flujo App sin UI) ─────────────────────────────────────────────

    private static void RunEndToEnd()
    {
        string root = Path.Combine(Path.GetTempPath(), "clasif-e2e-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            string[] names = { "cancion1.mp3", "cancion2.mp3", "documento1.pdf", "foto1.jpg", "tool.exe", "setup.dll" };
            foreach (var n in names)
                File.WriteAllText(Path.Combine(root, n), "x");

            var files = Directory.GetFiles(root).Select(f => Path.GetFileName(f)!).Where(f => !FileFilters.IsSystemFile(f)).ToList();
            Assert("E2E: filtro excluye .exe/.dll", files.Count == 4, $"got {files.Count}");

            var (mode, criterion, depth) = (ClassificationModes.Find("Música")!, "Género", 5);
            // E2E vía clasificador local (algoritmo determinista, sin LLM).
            var results = LocalClassifier.Classify(files, depth, Idioma.Español, criterion);
            Assert("E2E: local clasifica todo", results.Sum(r => r.Files.Count) == 4, string.Join(",", results.Select(r => $"{r.Category}({r.Files.Count})")));

            string dest = Path.Combine(root, "out");
            Directory.CreateDirectory(Path.Combine(root, "otra_carpeta"));
            File.WriteAllText(Path.Combine(root, "otra_carpeta", "extra1.mp3"), "x");
            var filePaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var n in names.Where(n => !FileFilters.IsSystemFile(n)))
                filePaths[n] = Path.Combine(root, n);
            filePaths["extra1.mp3"] = Path.Combine(root, "otra_carpeta", "extra1.mp3");
            var result = FileOrganizer.Organize(filePaths, dest, results, copy: false, CancellationToken.None);
            Assert("E2E: organizados", result.Processed == 4 && result.Errors == 0, $"p{result.Processed} e{result.Errors}");
            Assert("E2E: origen vacío tras mover", !File.Exists(Path.Combine(root, "cancion1.mp3")));
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }
    }

    // ── Dedup filter ───────────────────────────────────────────────────

    private static void RunDedupFilter()
    {
        // a.txt: 3 copias con distinto tamaño/fecha.
        var items = new List<(string Name, long Size, DateTime Date)>
        {
            ("a.txt", 100, new DateTime(2024, 1, 1)),
            ("a.txt", 200, new DateTime(2024, 6, 1)),
            ("a.txt", 300, new DateTime(2024, 3, 1)),
            ("b.txt", 50, new DateTime(2024, 2, 1)),
            ("b.txt", 70, new DateTime(2024, 4, 1)),
            ("c.txt", 10, new DateTime(2024, 1, 1)),
        };

        List<string> Keep(bool sameName, bool sizeEnabled, bool sizeKeepLargest, bool dateEnabled, bool dateKeepLargest) =>
            DedupFilter.Keep(items, x => x.Name, x => x.Size, x => x.Date,
                sameName, sizeEnabled, sizeKeepLargest, dateEnabled, dateKeepLargest)
                .Select(x => x.Name).ToList();

        var none = Keep(sameName: false, sizeEnabled: true, sizeKeepLargest: true, dateEnabled: true, dateKeepLargest: true);
        Assert("Dedup master off muestra todos", none.Count == 6, $"got {none.Count}");

        // Ejes desactivados → no se filtra nada dentro de cada grupo.
        var noAxes = Keep(sameName: true, sizeEnabled: false, sizeKeepLargest: true, dateEnabled: false, dateKeepLargest: true);
        Assert("Dedup sin ejes conserva todo el grupo", noAxes.Count == 6, string.Join(",", noAxes));

        // Tamaño Mayor → conserva el mayor de cada grupo (a de 300, b de 70).
        var bySize = Keep(sameName: true, sizeEnabled: true, sizeKeepLargest: true, dateEnabled: false, dateKeepLargest: true);
        Assert("Dedup mayor tamaño", bySize.Count == 3 && bySize.Count(n => n == "a.txt") == 1, string.Join(",", bySize));

        // Tamaño Menor → conserva el menor (a de 100, b de 50).
        var bySizeMin = Keep(sameName: true, sizeEnabled: true, sizeKeepLargest: false, dateEnabled: false, dateKeepLargest: true);
        Assert("Dedup menor tamaño", bySizeMin.Count == 3 && bySizeMin.Count(n => n == "a.txt") == 1, string.Join(",", bySizeMin));

        // Fecha Mayor → conserva el más reciente (a de jun, b de abr).
        var byDate = Keep(sameName: true, sizeEnabled: false, sizeKeepLargest: true, dateEnabled: true, dateKeepLargest: true);
        Assert("Dedup mayor fecha", byDate.Count == 3 && byDate.Count(n => n == "a.txt") == 1, string.Join(",", byDate));

        // Fecha Menor → conserva el más antiguo (a de ene, b de feb).
        var byDateMin = Keep(sameName: true, sizeEnabled: false, sizeKeepLargest: true, dateEnabled: true, dateKeepLargest: false);
        Assert("Dedup menor fecha", byDateMin.Count == 3 && byDateMin.Count(n => n == "a.txt") == 1, string.Join(",", byDateMin));

        // Combinación en serie: tamaño Menor luego fecha Menor → a de 100, b de 50.
        var both = Keep(sameName: true, sizeEnabled: true, sizeKeepLargest: false, dateEnabled: true, dateKeepLargest: false);
        Assert("Dedup tamaño+fecha", both.Count == 3, string.Join(",", both));

        // Clave normalizada: descuenta números de pista, respeta nombres que son solo números.
        Assert("NormalizeKey descuenta pista",
            DedupFilter.NormalizeKey("001. Wham! - Wake Me Up.mp3") == "Wham! - Wake Me Up.mp3" &&
            DedupFilter.NormalizeKey("42 - Wham! - Wake Me Up.mp3") == "Wham! - Wake Me Up.mp3" &&
            DedupFilter.NormalizeKey("01_Wake Me Up.mp3") == "Wake Me Up.mp3" &&
            DedupFilter.NormalizeKey("1984.mp3") == "1984.mp3",
            "NormalizeKey dio un resultado inesperado");

        // Misma canción en dos recopilatorios → mismo grupo y queda el mayor.
        var recos = new List<(string Name, long Size, DateTime Date)>
        {
            ("001. Wham! - Wake Me Up.mp3", 100, new DateTime(2024, 1, 1)),
            ("042. Wham! - Wake Me Up.mp3", 130, new DateTime(2024, 2, 1)),
        };
        var recosKept = DedupFilter.Keep(recos, x => x.Name, x => x.Size, x => x.Date, true, true, true, false, true)
            .Select(x => x.Name).ToList();
        Assert("Dedup ignora numero de pista",
            recosKept.Count == 1 && recosKept[0] == "042. Wham! - Wake Me Up.mp3",
            string.Join(",", recosKept));
    }
}