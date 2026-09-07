using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

/// <summary>Clasificación por lotes con IA (BYOK): asignación por chunk + consolidación final.</summary>
public sealed class BatchClassifier
{
    private readonly Func<string, Task<string>> _generate;
    private readonly int _batchSize;
    private readonly int _depth;

    public BatchClassifier(Func<string, Task<string>> generate, int batchSize = 20, int depth = 5)
    {
        _generate = generate;
        _batchSize = Math.Max(1, batchSize);
        _depth = Math.Max(1, depth);
    }

    public async Task<List<ClassificationResult>> ClassifyAsync(
        ClassificationMode mode, string criterion, Idioma idioma,
        IReadOnlyList<string> files, Action<string>? onStatus = null, CancellationToken ct = default)
    {
        var fileNames = files.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var assignments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var failed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        int total = fileNames.Count;
        int batchCount = (total + _batchSize - 1) / _batchSize;
        int batchIndex = 0;

        // Fase 1: asignación por lote.
        foreach (var chunk in Chunk(fileNames, _batchSize))
        {
            ct.ThrowIfCancellationRequested();
            batchIndex++;
            onStatus?.Invoke(string.Format(Translations.Get("BatchStatus", idioma), batchIndex, batchCount));

            var batchFiles = chunk.ToList();
            string prompt = PromptGenerator.AssignmentBatch(mode, criterion, idioma, batchFiles);
            try
            {
                string text = await _generate(prompt).ConfigureAwait(false);
                var parsed = ResponseParser.ParseBatchAssignments(text, batchFiles, idioma);
                foreach (var f in batchFiles)
                    if (parsed.TryGetValue(f, out string? cat)) assignments[f] = cat;
                foreach (var f in batchFiles)
                    if (!assignments.ContainsKey(f)) failed.Add(f);
            }
            catch (AiException)
            {
                foreach (var f in batchFiles) failed.Add(f);
            }
        }

        string others = Translations.Get("Otros", idioma);

        // Fase 2: consolidación (solo categorías que sí fueron asignadas).
        var categories = assignments
            .GroupBy(kv => kv.Value, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CategoryStat(g.Key, g.Count(), g.Take(2).Select(kv => kv.Key).ToArray()))
            .ToList();
        if (categories.Count > 0)
        {
            string prompt = PromptGenerator.Consolidate(categories, _depth, idioma);
            try
            {
                string text = await _generate(prompt).ConfigureAwait(false);
                var map = ResponseParser.ParseConsolidationMap(text, idioma);
                if (map != null)
                {
                    var sourceToFinal = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var kv in map)
                        foreach (var src in kv.Value)
                            sourceToFinal.TryAdd(src, kv.Key);

                    foreach (var f in assignments.Keys.ToList())
                    {
                        string src = assignments[f];
                        assignments[f] = sourceToFinal.TryGetValue(src, out string? fin) ? fin : others;
                    }
                }
            }
            catch (AiException)
            {
                // Consolidación fallida → se conservan las categorías crudas del lote.
            }
        }

        var groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (f, cat) in assignments)
        {
            string c = FileOrganizer.SaneateFolderName(cat);
            if (!groups.TryGetValue(c, out var list))
                groups[c] = list = new List<string>();
            list.Add(f);
        }
        var results = groups
            .Select(kv => new ClassificationResult(kv.Key, kv.Value.AsReadOnly()))
            .ToList();

        if (failed.Count > 0)
        {
            int idx = results.FindIndex(r => r.Category == others);
            if (idx >= 0)
            {
                results[idx] = new ClassificationResult(others, results[idx].Files.Concat(failed).ToList());
            }
            else
            {
                results.Add(new ClassificationResult(others, failed.ToList()));
            }
        }

        return LocalClassifier.CapToDepth(results, _depth, others);
    }

    private static IEnumerable<IEnumerable<string>> Chunk(IReadOnlyList<string> items, int size)
    {
        for (int i = 0; i < items.Count; i += size)
            yield return items.Skip(i).Take(size);
    }
}