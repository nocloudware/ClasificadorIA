using System.Text.Json.Serialization;

namespace ClasificadorIA.Models;

public enum AiScheme
{
    OpenAi,
    Anthropic,
    Gemini
}

public sealed class AiProvider
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AiScheme Scheme { get; set; } = AiScheme.OpenAi;
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public List<string> Models { get; set; } = new();
    public string SelectedModel { get; set; } = "";
    public string ApiKeyUrl { get; set; } = "";
    public double Temperature { get; set; } = 0.7;
    public bool JsonMode { get; set; } = true;

    /// <summary>Tope de tokens de salida por llamada. <see cref="MaxTokensConst.Max"/> = auto (calculado por cantidad de archivos).</summary>
    public int MaxTokens { get; set; } = MaxTokensConst.Max;

    /// <summary>Límites en vivo capturados al listar modelos (Gemini/Groq/OpenRouter). Transitorio, no se guarda.</summary>
    [JsonIgnore]
    public Dictionary<string, int> ModelOutputLimits { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Tope máximo real de salida del modelo actual (vivo o estático). null = desconocido.</summary>
    [JsonIgnore]
    public int? MaxOutputLimit { get; set; }

    /// <summary>Hosts locales (Ollama) no requieren API key.</summary>
    public bool RequiresApiKey
    {
        get
        {
            if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri))
                return true;
            return uri.Host is not ("localhost" or "127.0.0.1" or "::1");
        }
    }

    public AiProvider Clone() => new()
    {
        Id = Id, Name = Name, Scheme = Scheme, BaseUrl = BaseUrl, ApiKey = ApiKey,
        Models = new List<string>(Models), SelectedModel = SelectedModel,
        ApiKeyUrl = ApiKeyUrl, Temperature = Temperature, JsonMode = JsonMode, MaxTokens = MaxTokens,
        ModelOutputLimits = new Dictionary<string, int>(ModelOutputLimits),
        MaxOutputLimit = MaxOutputLimit
    };

    public static IReadOnlyList<AiProvider> DefaultPresets() => new List<AiProvider>
    {
        new()
        {
            Id = "openai", Name = "OpenAI", Scheme = AiScheme.OpenAi,
            BaseUrl = "https://api.openai.com/v1",
            Models = { "gpt-4o-mini", "gpt-4o", "gpt-4.1-mini" },
            ApiKeyUrl = "https://platform.openai.com/api-keys"
        },
        new()
        {
            Id = "claude", Name = "Claude (Anthropic)", Scheme = AiScheme.Anthropic,
            BaseUrl = "https://api.anthropic.com/v1",
            Models = { "claude-sonnet-4-5", "claude-opus-4-1", "claude-haiku-4-5" },
            ApiKeyUrl = "https://console.anthropic.com/settings/keys"
        },
        new()
        {
            Id = "deepseek", Name = "DeepSeek", Scheme = AiScheme.OpenAi,
            BaseUrl = "https://api.deepseek.com/v1",
            Models = { "deepseek-chat", "deepseek-reasoner" },
            ApiKeyUrl = "https://platform.deepseek.com/api_keys"
        },
        new()
        {
            Id = "gemini", Name = "Google Gemini", Scheme = AiScheme.Gemini,
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta",
            Models = { "gemini-2.5-flash", "gemini-2.5-pro" },
            ApiKeyUrl = "https://aistudio.google.com/apikey"
        },
        new()
        {
            Id = "groq", Name = "Groq", Scheme = AiScheme.OpenAi,
            BaseUrl = "https://api.groq.com/openai/v1",
            Models = { "llama-3.3-70b-versatile", "qwen-3-32b" },
            ApiKeyUrl = "https://console.groq.com/keys"
        },
        new()
        {
            Id = "ollama", Name = "Ollama (local)", Scheme = AiScheme.OpenAi,
            BaseUrl = "http://localhost:11434/v1",
            Models = { },
            ApiKeyUrl = "",
            JsonMode = false
        },
        new()
        {
            Id = "openrouter", Name = "OpenRouter", Scheme = AiScheme.OpenAi,
            BaseUrl = "https://openrouter.ai/api/v1",
            Models = { },
            ApiKeyUrl = "https://openrouter.ai/settings/keys"
        }
    };
}