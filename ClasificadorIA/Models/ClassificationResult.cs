namespace ClasificadorIA.Models;

public sealed record ClassificationResult(string Category, IReadOnlyList<string> Files);

public sealed record CategoryStat(string Name, int Count, string[] Examples);