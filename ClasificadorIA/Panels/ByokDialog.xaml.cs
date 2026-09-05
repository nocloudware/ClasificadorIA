using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using ClasificadorIA.Models;
using ClasificadorIA.Services;

namespace ClasificadorIA.Panels;

public partial class ByokDialog : Window
{
    private readonly ByokConfig _config;
    private readonly ByokConfigStore _store;
    private readonly AiClient _client;
    private readonly Idioma _idioma;
    private readonly ByokConfig _working = new();
    private readonly AiProvider _addMarker = new() { Id = "__add__", Name = "" };
    private bool _loading;

    public ByokDialog(ByokConfig config, ByokConfigStore store, AiClient client, Idioma idioma)
    {
        InitializeComponent();
        _config = config;
        _store = store;
        _client = client;
        _idioma = idioma;
        Loaded += (_, _) => Populate();

        _addMarker.Name = Translations.Get("AddCustomProvider", _idioma);
        SchemeCombo.ItemsSource = Enum.GetValues<AiScheme>();
        DataContext = this;
    }

    // ── Etiquetas localizadas ─────────────────────────────────────────

    public string ByokDialogTitle => Translations.Get("ByokTitle", _idioma);
    public string ByokDialogSubtitle => Translations.Get("ByokDescription", _idioma);
    public string ProviderLabel => Translations.Get("Provider", _idioma);
    public string NameLabel => Translations.Get("Name", _idioma);
    public string SchemeLabel => Translations.Get("Scheme", _idioma);
    public string BaseUrlLabel => Translations.Get("BaseUrl", _idioma);
    public string ApiKeyLabel => Translations.Get("ApiKey", _idioma);
    public string ModelLabel => Translations.Get("Model", _idioma);
    public string ReloadModelsLabel => Translations.Get("ReloadModels", _idioma);
    public string ModelHint => Translations.Get("ModelHint", _idioma);
    public string TemperatureLabel => Translations.Get("Temperature", _idioma);
    public string DeleteLabel => Translations.Get("Delete", _idioma);
    public string TestConnectionLabel => Translations.Get("TestConnection", _idioma);
    public string SaveLabel => Translations.Get("Save", _idioma);
    public string CancelLabel => Translations.Get("Cancel", _idioma);

    // ── Carga inicial ──────────────────────────────────────────────────

    private void Populate()
    {
        _working.Providers = new List<AiProvider>(_config.Providers.Select(p => p.Clone()));
        _working.ActiveProviderId = _config.ActiveProviderId;
        RefreshProviderCombo();
    }

    private static readonly HashSet<string> PresetIds = new(
        AiProvider.DefaultPresets().Select(p => p.Id));

    private static bool IsPreset(AiProvider p) => p.Id != "__add__" && PresetIds.Contains(p.Id);

    private AiProvider? SelectedProvider => ProviderCombo.SelectedItem as AiProvider;

    private void RefreshProviderCombo(AiProvider? select = null)
    {
        ProviderCombo.SelectionChanged -= ProviderCombo_SelectionChanged;
        var items = new List<AiProvider>(_working.Providers) { _addMarker };
        ProviderCombo.ItemsSource = items;
        ProviderCombo.SelectionChanged += ProviderCombo_SelectionChanged;

        _loading = true;
        if (select != null)
        {
            ProviderCombo.SelectedItem = select;
        }
        else
        {
            var active = _working.Providers.FirstOrDefault(p => p.Id == _working.ActiveProviderId);
            ProviderCombo.SelectedItem = active ?? _working.Providers.FirstOrDefault();
        }
        if (ProviderCombo.SelectedItem is AiProvider sel)
            LoadProviderIntoFields(sel);
        _loading = false;
    }

    private void ProviderCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_loading) return;
        if (ProviderCombo.SelectedItem is not AiProvider p) return;
        if (p.Id == _addMarker.Id)
        {
            var np = new AiProvider
            {
                Name = Translations.Get("NewProviderDefaultName", _idioma),
                BaseUrl = "https://",
                Temperature = 0.7,
            };
            _working.Providers.Add(np);
            RefreshProviderCombo(np);
            return;
        }
        _loading = true;
        LoadProviderIntoFields(p);
        _loading = false;
    }

    private void LoadProviderIntoFields(AiProvider p)
    {
        bool preset = IsPreset(p);
        _loading = true;
        NameBox.Text = p.Name;
        SchemeCombo.SelectedItem = p.Scheme;
        SchemeCombo.IsEnabled = !preset;
        BaseUrlBox.Text = p.BaseUrl;
        ApiKeyBox.Text = p.ApiKey;
        if (preset)
        {
            ApiKeyLink.Visibility = Visibility.Visible;
            ApiKeyUrlBox.Visibility = Visibility.Collapsed;
            ApiKeyHyperlink.NavigateUri = new Uri(p.ApiKeyUrl);
            ApiKeyHyperlink.Inlines.Clear();
            ApiKeyHyperlink.Inlines.Add(Translations.Get("GetApiKey", _idioma));
        }
        else
        {
            ApiKeyLink.Visibility = Visibility.Collapsed;
            ApiKeyUrlBox.Visibility = Visibility.Visible;
            ApiKeyUrlBox.Text = p.ApiKeyUrl;
        }
        TemperatureSlider.Value = p.Temperature;
        RebindModelCombo(p);
        StatusText.Text = "";
        _loading = false;
    }

    private void RebindModelCombo(AiProvider p)
    {
        ModelCombo.ItemsSource = p.Models;
        if (!string.IsNullOrEmpty(p.SelectedModel) && p.Models.Contains(p.SelectedModel, StringComparer.OrdinalIgnoreCase))
            ModelCombo.SelectedItem = p.SelectedModel;
        else
        {
            ModelCombo.SelectedItem = null;
            ModelCombo.Text = p.SelectedModel;
        }
    }

    private void ApplyFieldEdits(AiProvider p)
    {
        p.Name = NameBox.Text.Trim();
        if (SchemeCombo.SelectedItem is AiScheme s && !IsPreset(p))
            p.Scheme = s;
        p.BaseUrl = BaseUrlBox.Text.Trim();
        p.ApiKey = ApiKeyBox.Text;
        p.ApiKeyUrl = (IsPreset(p) ? p.ApiKeyUrl : ApiKeyUrlBox.Text.Trim());
        p.Temperature = Math.Round(TemperatureSlider.Value, 2);
    }

    // ── Modelos / prueba ───────────────────────────────────────────────

    private async Task ReloadModelsAsync()
    {
        var p = SelectedProvider;
        if (p == null || p.Id == _addMarker.Id) return;
        ApplyFieldEdits(p);
        if (p.RequiresApiKey && string.IsNullOrWhiteSpace(p.ApiKey))
        {
            StatusText.Text = string.Format(Translations.Get("ProviderNeedsKey", _idioma), p.Name);
            return;
        }
        ReloadModelsButton.IsEnabled = false;
        StatusText.Text = Translations.Get("LoadingModels", _idioma);
        try
        {
            var models = await _client.ListModelsAsync(p);
            p.Models = models.ToList();
            RebindModelCombo(p);
            StatusText.Text = string.Format(Translations.Get("ModelsLoaded", _idioma), models.Count);
        }
        catch (AiException ex)
        {
            StatusText.Text = Translations.AiErrorMessage(ex, _idioma);
        }
        finally
        {
            ReloadModelsButton.IsEnabled = true;
        }
    }

    private void ReloadModelsButton_Click(object sender, RoutedEventArgs e) => _ = ReloadModelsAsync();

    private void TestButton_Click(object sender, RoutedEventArgs e) => _ = ReloadModelsAsync();

    private async void ApiKeyHyperlink_Click(object sender, RoutedEventArgs e)
    {
        var p = SelectedProvider;
        if (p == null || string.IsNullOrEmpty(p.ApiKeyUrl)) return;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = p.ApiKeyUrl, UseShellExecute = true });
        }
        catch (System.ComponentModel.Win32Exception) { }
    }

    // ── Eliminar / Guardar / Cancelar ──────────────────────────────────

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var p = SelectedProvider;
        if (p == null || p.Id == _addMarker.Id) return;
        if (_working.Providers.Count <= 1)
        {
            StatusText.Text = Translations.Get("ProvidersEmpty", _idioma);
            return;
        }
        if (MessageBox.Show(this, string.Format(Translations.Get("DeleteByokConfirm", _idioma), p.Name),
                Translations.Get("ByokTitle", _idioma), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        _working.Providers.Remove(p);
        if (_working.ActiveProviderId == p.Id)
            _working.ActiveProviderId = _working.Providers[0].Id;
        RefreshProviderCombo();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var p = SelectedProvider;
        if (p == null || p.Id == _addMarker.Id)
        {
            StatusText.Text = Translations.Get("ProvidersEmpty", _idioma);
            return;
        }
        ApplyFieldEdits(p);
        if (string.IsNullOrWhiteSpace(p.Name) ||
            !Uri.TryCreate(p.BaseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            StatusText.Text = Translations.Get("ByokNeedsFields", _idioma);
            return;
        }
        if (p.RequiresApiKey && string.IsNullOrWhiteSpace(p.ApiKey))
        {
            StatusText.Text = string.Format(Translations.Get("ProviderNeedsKey", _idioma), p.Name);
            return;
        }
        string model = ModelCombo.Text?.Trim() ?? "";
        if (model.Length == 0)
        {
            StatusText.Text = Translations.Get("ErrNoModel", _idioma);
            return;
        }
        if (!p.Models.Contains(model, StringComparer.OrdinalIgnoreCase))
            p.Models.Insert(0, model);
        p.SelectedModel = model;
        p.Temperature = Math.Round(TemperatureSlider.Value, 2);

        _config.Providers = new List<AiProvider>(_working.Providers);
        _config.ActiveProviderId = p.Id;
        _store.Save(_config);
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void TemperatureSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TemperatureValueText != null)
            TemperatureValueText.Text = Math.Round(e.NewValue, 2).ToString("0.00");
    }
}