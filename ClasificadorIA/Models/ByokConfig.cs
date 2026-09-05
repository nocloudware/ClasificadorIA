using System.Text.Json.Serialization;

namespace ClasificadorIA.Models;

public sealed class ByokConfig
{
    public List<AiProvider> Providers { get; set; } = new();
    public string? ActiveProviderId { get; set; }

    [JsonIgnore]
    public AiProvider? ActiveProvider =>
        Providers.FirstOrDefault(p => string.Equals(p.Id, ActiveProviderId, StringComparison.OrdinalIgnoreCase));
}