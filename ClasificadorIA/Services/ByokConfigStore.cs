using System.IO;
using System.Text.Json;
using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

public sealed class ByokConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _path;

    public ByokConfigStore(string? path = null)
    {
        _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClasificadorIA", "byok.json");
    }

    public string ConfigPath => _path;

    public ByokConfig Load()
    {
        var config = TryReadFile();
        if (config == null)
            config = Default();

        EnsurePresets(config);
        if (config.ActiveProvider == null)
            config.ActiveProviderId = config.Providers.FirstOrDefault()?.Id;
        return config;
    }

    public void Save(ByokConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(config, JsonOptions));
    }

    private ByokConfig? TryReadFile()
    {
        if (!File.Exists(_path))
            return null;
        try
        {
            var config = JsonSerializer.Deserialize<ByokConfig>(File.ReadAllText(_path));
            if (config == null) return null;
            foreach (var p in config.Providers)
                if (string.IsNullOrWhiteSpace(p.Id))
                    p.Id = Guid.NewGuid().ToString("N");
            return config;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ByokConfig Default() => new()
    {
        Providers = AiProvider.DefaultPresets().ToList(),
        ActiveProviderId = "openai"
    };

    private static void EnsurePresets(ByokConfig config)
    {
        var presets = AiProvider.DefaultPresets();
        foreach (var preset in presets)
        {
            if (config.Providers.All(p => !string.Equals(p.Id, preset.Id, StringComparison.OrdinalIgnoreCase)))
                config.Providers.Add(preset);
        }
    }
}