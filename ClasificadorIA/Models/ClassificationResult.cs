namespace ClasificadorIA.Models;

public sealed record ClassificationResult(string Category, IReadOnlyList<string> Files);