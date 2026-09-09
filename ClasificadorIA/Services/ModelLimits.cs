namespace ClasificadorIA.Services;

using ClasificadorIA.Models;

/// <summary>Resolución del tope máximo de tokens de salida de un modelo.
/// Fuentes, en orden: límite en vivo capturado al listar modelos (Gemini/Groq/OpenRouter),
/// luego tabla estática de valores documentados por proveedor.</summary>
public static class ModelLimits
{
    // Sensibles a mayúsculas: se buscan por subcadena, las específicas van primero.
    private static readonly (string Sub, int Max)[] Known =
    {
        // DeepSeek (docs oficiales: deepseek-v4 384K salida; reasoner 64K; chat 8K)
        ("deepseek-v4", 384_000),
        ("deepseek-reasoner", 65_536),
        ("deepseek-chat", 8_192),
        // OpenAI
        ("gpt-4o-mini", 16_384),
        ("gpt-4o", 16_384),
        ("gpt-4.1", 32_768),
        ("gpt-4-turbo", 4_096),
        ("gpt-4", 8_192),
        ("gpt-3.5", 4_096),
        // Anthropic (modelo → ventana → salida; opus/sonnet 1M = 128K, sonnet 4.5 = 64K, haiku = 8K)
        ("claude-opus-5", 128_000),
        ("claude-sonnet-5", 128_000),
        ("claude-opus-4", 128_000),
        ("claude-sonnet-4-5", 64_000),
        ("claude-haiku-4-5", 8_192),
        // Gemini (el límite en vivo lo pisa; la tabla cubre gemini sin listar)
        ("gemini-2.5-pro", 65_536),
        ("gemini-2.5-flash", 8_192),
        // Genéricos locales
        ("llama-3.3-70b", 32_768),
        ("qwen-3", 32_768),
        ("llama3", 4_096),
    };

    /// <summary>Límite en vivo o estático para un modelo. null = desconocido (no limitar).</summary>
    public static int? Resolve(AiProvider provider, string model)
    {
        if (provider.ModelOutputLimits.TryGetValue(model, out int live))
            return live;
        return LookupStatic(model);
    }

    public static int? LookupStatic(string model)
    {
        if (string.IsNullOrWhiteSpace(model)) return null;
        foreach (var (sub, max) in Known)
            if (model.Contains(sub, StringComparison.OrdinalIgnoreCase))
                return max;
        return null;
    }
}