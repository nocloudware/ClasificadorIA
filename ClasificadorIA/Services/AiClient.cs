using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

/// <summary>Cliente multiproveedor propio (0 NuGets) con esquemas wire openai/anthropic/gemini.</summary>
public sealed class AiClient
{
    private readonly HttpClient _http;

    public AiClient(HttpClient? http = null)
    {
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
    }

    // ── Generar respuesta ──────────────────────────────────────────────

    public async Task<string> GenerateAsync(AiProvider provider, string prompt, CancellationToken ct = default)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        string model = ResolveModel(provider);

        using var req = BuildChatRequest(provider, model, prompt);
        try
        {
            using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false);
            string body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
                throw BuildException(resp.StatusCode, body);
            return ExtractResponseText(provider, body);
        }
        catch (AiException)
        {
            throw;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AiException(0, "timeout", "Timeout de conexión.");
        }
        catch (HttpRequestException ex)
        {
            throw new AiException(0, "network", ex.Message);
        }
    }

    private static string ResolveModel(AiProvider provider)
    {
        string model = provider.SelectedModel?.Trim() ?? "";
        if (model.Length == 0)
            model = provider.Models.FirstOrDefault() ?? "";
        if (model.Length == 0)
            throw new AiException(0, "no_model", "No hay modelo seleccionado.");
        return model;
    }

    public HttpRequestMessage BuildChatRequest(AiProvider provider, string model, string prompt)
    {
        if (provider.RequiresApiKey && string.IsNullOrWhiteSpace(provider.ApiKey))
            throw new AiException(0, "missing_key", "API key requerida.");

        string url = JoinUrl(provider.BaseUrl, provider.Scheme == AiScheme.Gemini
            ? $"models/{Uri.EscapeDataString(model)}:generateContent"
            : "chat/completions");

        var req = new HttpRequestMessage(HttpMethod.Post, url);
        ApplyAuth(provider, req);
        req.Content = new StringContent(BuildPayloadRaw(provider, model, prompt), Encoding.UTF8, "application/json");
        return req;
    }

    private static void ApplyAuth(AiProvider provider, HttpRequestMessage req)
    {
        switch (provider.Scheme)
        {
            case AiScheme.Anthropic:
                req.Headers.TryAddWithoutValidation("x-api-key", provider.ApiKey);
                req.Headers.TryAddWithoutValidation("anthropic-version", "2023-06-01");
                break;
            case AiScheme.Gemini:
                req.Headers.TryAddWithoutValidation("x-goog-api-key", provider.ApiKey);
                break;
            default:
                if (!string.IsNullOrWhiteSpace(provider.ApiKey))
                    req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + provider.ApiKey);
                break;
        }
    }

    // ── Payloads wire ──────────────────────────────────────────────────

    public string BuildPayload(AiProvider provider, string prompt)
    {
        string model = ResolveModel(provider);
        return BuildPayloadRaw(provider, model, prompt);
    }

    private string BuildPayloadRaw(AiProvider provider, string model, string prompt)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return provider.Scheme switch
        {
            AiScheme.Anthropic => JsonSerializer.Serialize(new
            {
                model,
                max_tokens = 4096,
                temperature = provider.Temperature,
                messages = new[] { new { role = "user", content = prompt } }
            }, options),
            AiScheme.Gemini => JsonSerializer.Serialize(new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new
                {
                    temperature = provider.Temperature,
                    responseMimeType = "application/json"
                }
            }, options),
            // OpenAI-compatible (openai/deepseek/groq/openrouter/ollama)
            _ => BuildOpenAiPayload(provider, model, prompt)
        };
    }

    private static string BuildOpenAiPayload(AiProvider provider, string model, string prompt)
    {
        var body = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = new[] { new { role = "user", content = prompt } },
            ["temperature"] = provider.Temperature
        };
        if (provider.JsonMode)
            body["response_format"] = new { type = "json_object" };
        return JsonSerializer.Serialize(body);
    }

    // ── Extraer texto de respuesta ─────────────────────────────────────

    public string ExtractResponseText(AiProvider provider, string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (TryReadError(root, out string? errCode, out string? errMessage, out int errStatus))
            throw new AiException(errStatus, errCode, errMessage ?? json);

        return provider.Scheme switch
        {
            AiScheme.Anthropic => ReadAnthropicText(root),
            AiScheme.Gemini => ReadGeminiText(root),
            _ => ReadOpenAiText(root)
        };
    }

    private static string ReadOpenAiText(JsonElement root)
    {
        if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            var message = choices[0].GetProperty("message");
            if (message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                return content.GetString() ?? "";
        }
        throw new AiException(200, "bad_response", "No se pudo leer content de la respuesta.");
    }

    private static string ReadAnthropicText(JsonElement root)
    {
        if (root.TryGetProperty("content", out var content) && content.GetArrayLength() > 0)
        {
            foreach (var block in content.EnumerateArray())
                if (block.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                    return text.GetString() ?? "";
        }
        throw new AiException(200, "bad_response", "No se pudo leer text de la respuesta.");
    }

    private static string ReadGeminiText(JsonElement root)
    {
        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];
            if (candidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                return text.GetString() ?? "";
        }
        throw new AiException(200, "bad_response", "No se pudo leer parts[0].text de la respuesta.");
    }

    // ── Errores ────────────────────────────────────────────────────────

    private static bool TryReadError(JsonElement root, out string? code, out string? message, out int status)
    {
        code = null;
        message = null;
        status = 0;

        // openai / gemini: { "error": { "message", "code"|"status", ... } }
        // anthropic también expone { "error": { "type", "message", "status_code" } }
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("error", out var errObj) &&
            errObj.ValueKind == JsonValueKind.Object)
        {
            TryGetString(errObj, "message", out message);
            TryGetString(errObj, "code", out code);
            if (code == null) TryGetString(errObj, "status", out code);
            if (code == null) TryGetString(errObj, "type", out code);
            TryGetNumber(errObj, "code", out status);
            if (status == 0) TryGetNumber(errObj, "status", out status);
            if (status == 0) TryGetNumber(errObj, "status_code", out status);
            return true;
        }

        // anthropic: { "type": "error", "error": { "type", "message", "status_code" } }
        if (root.TryGetProperty("type", out var typeEl) &&
            typeEl.ValueKind == JsonValueKind.String && typeEl.GetString() == "error" &&
            root.TryGetProperty("error", out var antErr) && antErr.ValueKind == JsonValueKind.Object)
        {
            TryGetString(antErr, "message", out message);
            TryGetString(antErr, "type", out code);
            TryGetNumber(antErr, "status_code", out status);
            return true;
        }
        return false;
    }

    private static void TryGetNumber(JsonElement el, string name, out int value)
    {
        if (el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number)
        {
            value = v.GetInt32();
            return;
        }
        value = 0;
    }

    private static bool TryGetString(JsonElement el, string name, out string? value)
    {
        if (el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String)
        {
            value = v.GetString();
            return true;
        }
        value = null;
        return false;
    }

    private AiException BuildException(HttpStatusCode status, string body)
    {
        string? code = null;
        string? message = null;
        int foundStatus = 0;
        try
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                using var doc = JsonDocument.Parse(body);
                if (TryReadError(doc.RootElement, out code, out message, out foundStatus))
                {
                }
            }
        }
        catch (JsonException) { }

        if (foundStatus > 0) status = (HttpStatusCode)foundStatus;
        string fallbackCode = status switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "auth",
            HttpStatusCode.NotFound => "not_found",
            (HttpStatusCode)429 => "rate_limit",
            _ => "invalid_request"
        };
        code ??= fallbackCode;
        message ??= $"HTTP {(int)status}";

        // Anthropic expone 401 para key inválida too
        if ((int)status is 401 or 403) code = "auth";
        return new AiException((int)status, code, message);
    }

    // ── Listar modelos ─────────────────────────────────────────────────

    public async Task<IReadOnlyList<string>> ListModelsAsync(AiProvider provider, CancellationToken ct = default)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        if (provider.RequiresApiKey && string.IsNullOrWhiteSpace(provider.ApiKey))
            throw new AiException(0, "missing_key", "API key requerida.");

        var req = new HttpRequestMessage(HttpMethod.Get, BuildListModelsUrl(provider));
        ApplyAuth(provider, req);
        try
        {
            using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false);
            string body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
                throw BuildException(resp.StatusCode, body);
            return ExtractModels(provider, body);
        }
        catch (AiException) { throw; }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AiException(0, "timeout", "Timeout de conexión.");
        }
        catch (HttpRequestException ex)
        {
            throw new AiException(0, "network", ex.Message);
        }
    }

    public Uri BuildListModelsUrl(AiProvider provider)
    {
        string baseUrl = provider.BaseUrl.TrimEnd('/');
        if (provider.Scheme == AiScheme.Gemini)
            return new Uri(JoinUrl(baseUrl, "models"));
        // ollama: lista bajo /api/tags en la raíz del servidor
        if (IsLocalBase(provider))
        {
            if (baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
                baseUrl = baseUrl[..^3];
            return new Uri(JoinUrl(baseUrl, "api/tags"));
        }
        return new Uri(JoinUrl(baseUrl, "models"));
    }

    private static IReadOnlyList<string> ExtractModels(AiProvider provider, string json)
    {
        var names = new List<string>();
        using var doc = JsonDocument.Parse(json);
        if (provider.Scheme == AiScheme.Gemini)
        {
            if (doc.RootElement.TryGetProperty("models", out var models) && models.ValueKind == JsonValueKind.Array)
            {
                foreach (var m in models.EnumerateArray())
                {
                    if (TryGetString(m, "name", out string? name) && !string.IsNullOrWhiteSpace(name))
                        names.Add(name.StartsWith("models/") ? name["models/".Length..] : name);
                }
            }
        }
        else if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var m in data.EnumerateArray())
                if (TryGetString(m, "id", out string? id) && !string.IsNullOrWhiteSpace(id))
                    names.Add(id);
        }
        else if (provider.Scheme == AiScheme.OpenAi && IsLocalBase(provider) &&
                 doc.RootElement.TryGetProperty("models", out var tags) && tags.ValueKind == JsonValueKind.Array)
        {
            foreach (var m in tags.EnumerateArray())
                if (TryGetString(m, "name", out string? name) && !string.IsNullOrWhiteSpace(name))
                    names.Add(name);
        }

        return names.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private static string JoinUrl(string baseUrl, string path)
    {
        if (baseUrl.EndsWith('/'))
            baseUrl = baseUrl[..^1];
        if (path.StartsWith('/'))
            path = path[1..];
        return $"{baseUrl}/{path}";
    }

    internal static bool IsLocalBase(AiProvider provider)
    {
        if (provider.Id?.Equals("ollama", StringComparison.OrdinalIgnoreCase) == true)
            return true;
        if (!Uri.TryCreate(provider.BaseUrl, UriKind.Absolute, out var uri))
            return false;
        return uri.Host is "localhost" or "127.0.0.1" or "::1";
    }

    public string GetSelectedModel(AiProvider provider) => ResolveModel(provider);
}