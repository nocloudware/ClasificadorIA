using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ClasificadorIA.Models;
using ClasificadorIA.Panels;
using ClasificadorIA.Services;
using NoCloudware.UI.Core.Controls;
using NoCloudware.UI.Core.Services;
using NoCloudware.UI.Core.ViewModels;

namespace ClasificadorIA;

public partial class App : System.Windows.Application
{
    private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

    private ShellWindow? _window;
    private OptionsPanel? _options;
    private readonly ThemeService _themeService = new();
    private readonly ByokConfigStore _byokStore = new();
    private readonly ByokConfig _byok = new();
    private readonly AiClient _aiClient = new();

    private List<ClassificationResult> _results = new();
    private string _currentPrompt = "";
    private CancellationTokenSource? _organizeCts;
    private bool _organizing;
    private int _organizeTotal;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Contains("--selftest"))
        {
            Shutdown(SelfTest.Run() ? 0 : 1);
            return;
        }

        var (dark, lang, output) = LoadPreferences();
        _themeService.ApplyTheme(dark);
        Translations.Current = lang;

        var loaded = _byokStore.Load();
        _byok.Providers = loaded.Providers;
        _byok.ActiveProviderId = loaded.ActiveProviderId;

        InitWindow(output);
        _options = new OptionsPanel();
        _window!.MainControl.OptionsContent.Content = _options;
        WireEvents();
        ApplyLanguage();
        _window!.Show();
    }

    // ── Preferencias ──────────────────────────────────────────────────

    private (bool dark, Idioma lang, string? output) LoadPreferences()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return (true, Idioma.Español, null);
            var json = File.ReadAllText(SettingsPath);
            using var doc = JsonDocument.Parse(json);
            var s = doc.RootElement.GetProperty("Settings");
            bool dark = s.TryGetProperty("DarkTheme", out var t) ? t.GetBoolean() : true;
            Idioma lang = s.TryGetProperty("Language", out var l) && l.GetString() == "en" ? Idioma.Inglés : Idioma.Español;
            string? output = s.TryGetProperty("DefaultOutputPath", out var o) ? o.GetString() : null;
            return (dark, lang, output);
        }
        catch { return (true, Idioma.Español, null); }
    }

    private void SavePreferences()
    {
        try
        {
            string lang = Translations.Current == Idioma.Inglés ? "en" : "es";
            string? output = _window?.MainControl.OutputFolderText;
            var prefs = new { Settings = new { DarkTheme = _themeService.IsDarkTheme, Language = lang, DefaultOutputPath = output } };
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(prefs, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    // ── Ventana ───────────────────────────────────────────────────────

    private void InitWindow(string? savedOutput)
    {
        _window = new ShellWindow
        {
            Width = 1200, Height = 760,
            WindowMinWidth = 900, WindowMinHeight = 600,
            WindowIcon = LoadIcon()
        };
        _window.MainControl.AppTitle = Translations.Get("AppTitle");
        _window.MainControl.OptionsPanelMinWidth = 340;
        _window.MainControl.OutputFolderText = !string.IsNullOrEmpty(savedOutput)
            ? savedOutput
            : Translations.Get("OutputFolderDefault");

        var flagBase = "pack://application:,,,/NoCloudware.UI.Core;component/Assets/Flags/";
        _window.LanguageSelector.Languages = new System.Collections.ObjectModel.ObservableCollection<LanguageItem>
        {
            new("Español", $"{flagBase}flag-es.png", "es"),
            new("English", $"{flagBase}flag-uk.png", "en")
        };
        _window.LanguageSelector.ComboMaxWidth = 36;
        _window.LanguageSelector.SetLanguage(Translations.Current == Idioma.Inglés ? "en" : "es");
    }

    private static BitmapImage LoadIcon()
    {
        var icon = new BitmapImage();
        icon.BeginInit();
        icon.UriSource = new Uri("pack://application:,,,/ClasificadorIA;component/Assets/clasificadoria.png");
        icon.CacheOption = BitmapCacheOption.OnLoad;
        icon.EndInit();
        return icon;
    }

    // ── Idioma ────────────────────────────────────────────────────────

    private void ApplyLanguage()
    {
        if (_window == null) return;
        var t = Translations.Current;

        _window.Title = Translations.Get("AppTitle");
        _window.DropText = Translations.Get("DropText");
        _window.ActionButtonText = Translations.Get("ActionButton");
        _window.AboutButtonText = Translations.Get("AboutButton");
        _window.DonateButtonText = Translations.Get("DonateButton");
        _window.ExitButtonText = Translations.Get("ExitButton");

        _window.MainControl.FileListHeader = Translations.Get("FileListHeader");
        _window.MainControl.AppTagline = Translations.Get("AppTagline");
        _window.MainControl.SelectFilesButtonText = Translations.Get("SelectFilesBtn");
        _window.MainControl.ChangeButtonText = Translations.Get("ChangeBtn");

        _window.MainControl.StatusBar.TotalLabel = Translations.Get("StatusTotal");
        _window.MainControl.StatusBar.ProcessedLabel = Translations.Get("StatusProcessed");
        _window.MainControl.StatusBar.PendingLabel = Translations.Get("StatusPending");
        _window.MainControl.StatusBar.ErrorsLabel = Translations.Get("StatusErrors");

        _window.FileListBox.RemoveMenuItemText = Translations.Get("Remove");
        _window.FileListBox.ClearAllMenuItemText = Translations.Get("ClearAll");

        if (string.IsNullOrEmpty(_window.MainControl.OutputFolderText) ||
            _window.MainControl.OutputFolderText == Translations.Get("OutputFolderDefault", t))
            _window.MainControl.OutputFolderText = Translations.Get("OutputFolderDefault");

        if (_organizing)
            _window.MainControl.ActionButtonText = Translations.Get("Cancel");

        _window.MainControl.UpdateCounters();
    }

    private void WireEvents()
    {
        _window!.ExitClick += (_, _) => _window!.Close();
        _window.ActionClick += OnActionClicked;
        _window.FilesDropped += (_, args) =>
        {
            if (args is FilesDroppedEventArgs f)
                AddFiles(f.FilePaths);
        };
        _window.MainControl.OutputFolderChanged += (_, _) => SavePreferences();

        _window.ThemeToggle.ThemeToggled += (_, args) =>
        {
            if (args is ThemeToggledEventArgs a)
            {
                _themeService.ApplyTheme(a.IsDarkTheme);
                SavePreferences();
            }
        };

        _window.LanguageSelector.LanguageChanged += (_, args) =>
        {
            if (args is LanguageChangedEventArgs a)
            {
                Translations.Current = a.CultureCode == "en" ? Idioma.Inglés : Idioma.Español;
                ApplyLanguage();
                SavePreferences();
            }
        };

        _window.DonateClick += (_, _) => new DonationService("https://nocloudware.com/donate.html").OpenDonationPage();

        _window.AboutClick += (_, _) => ShowAboutDialog();

        _window.Closing += (_, _) =>
        {
            _organizeCts?.Cancel();
            SavePreferences();
        };

        _options!.ChangeBtn.Click += (_, _) => OpenSourceFolder();
        _options.GeneratePromptBtn.Click += (_, _) => RegeneratePrompt();
        _options.CopyPromptBtn.Click += (_, _) => CopyPrompt();
        _options.PasteResponseBtn.Click += (_, _) => PasteResponse();
        _options.LoadResponseBtn.Click += (_, _) => LoadResponse();
        _options.ClassifyBtn.Click += (_, _) => _ = ClassifyAsync();
        _options.ByokButton.Click += (_, _) => OpenByokDialog();

        _options.ModeCombo.SelectionChanged += (_, _) => RegeneratePrompt();
        _options.CriterionCombo.SelectionChanged += (_, _) => RegeneratePrompt();
        _options.DepthCombo.SelectionChanged += (_, _) => RegeneratePrompt();
        _options.ResponseBox.TextChanged += (_, _) => _results = new();
    }

    // ── Archivos ──────────────────────────────────────────────────────

    private void AddFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (FileFilters.IsSystemFile(Path.GetFileName(path))) continue;
            if (_window!.Files.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase))) continue;
            _window.Files.Add(new BaseFileItem
            {
                FilePath = path,
                FileName = Path.GetFileName(path),
                FileSize = new FileInfo(path).Length
            });
        }
        ResetResults();
        _window!.MainControl.UpdateCounters();
        RegeneratePrompt();
    }

    private void OpenSourceFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = Translations.Get("SourceFolder")
        };
        if (dialog.ShowDialog() != true) return;
        if (!Directory.Exists(dialog.FolderName))
        {
            MessageBox.Show(Translations.Get("FolderNotExist"), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var added = Directory.GetFiles(dialog.FolderName)
            .Where(f => !FileFilters.IsSystemFile(Path.GetFileName(f)))
            .ToArray();
        AddFiles(added);
    }

    // ── Prompt ────────────────────────────────────────────────────────

    private (ClassificationMode mode, string criterion, int depth) GetOptions()
    {
        string? modeKey = (_options?.ModeCombo.SelectedItem as ComboBoxItem)?.Tag as string;
        string criterion = (_options?.CriterionCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "Tema";
        int depth = (_options?.DepthCombo.SelectedItem as ComboBoxItem)?.Tag is int d ? d : 5;
        return (ClassificationModes.Find(modeKey ?? "") ?? ClassificationModes.Default, criterion, depth);
    }

    private string[] GetFileNames() =>
        _window!.Files.Where(f => !FileFilters.IsSystemFile(f.FileName)).Select(f => f.FileName).ToArray();

    private void RegeneratePrompt()
    {
        var (mode, criterion, depth) = GetOptions();
        var files = GetFileNames();
        _currentPrompt = files.Length > 0
            ? PromptGenerator.Generate(mode, criterion, depth, Translations.Current, files)
            : "";
    }

    private void CopyPrompt()
    {
        if (string.IsNullOrEmpty(_currentPrompt))
        {
            MessageBox.Show(Translations.Get("PromptEmpty"), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            Clipboard.SetText(_currentPrompt);
            ShowStatus(Translations.Get("PromptCopied"));
        }
        catch { }
    }

    private void PasteResponse()
    {
        try
        {
            if (Clipboard.ContainsText())
                _options!.ResponseBox.Text = Clipboard.GetText();
            else
                MessageBox.Show(Translations.Get("NoneInClipboard"), Translations.Get("Error"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch { }
    }

    private void LoadResponse()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Texto|*.txt;*.json" };
        if (dialog.ShowDialog() == true)
        {
            try { _options!.ResponseBox.Text = File.ReadAllText(dialog.FileName); }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Translations.Get("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // ── Clasificación ─────────────────────────────────────────────────

    private async Task ClassifyAsync()
    {
        var files = GetFileNames();
        if (files.Length == 0)
        {
            MessageBox.Show(Translations.Get("NoFilesToClassify"), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_options!.MethodAuto.IsChecked == true)
            await ClassifyAuto(files);
        else
            ClassifyManual(files);
    }

    private void ClassifyManual(string[] files)
    {
        string text = _options!.ResponseBox.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            MessageBox.Show(Translations.Get("PasteFirst"), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var results = ResponseParser.Parse(text, files, Translations.Current);
        if (results.Count == 0)
        {
            string key = text.Contains('{') ? "NoValidCategories" : "JsonNotFound";
            MessageBox.Show(Translations.Get(key), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        SetResults(results);
    }

    private async Task ClassifyAuto(string[] files)
    {
        var provider = _byok.ActiveProvider;
        if (provider == null || _byok.Providers.Count == 0)
        {
            OpenByokDialog();
            if (_byok.ActiveProvider == null) return;
        }

        var (mode, criterion, depth) = GetOptions();
        string prompt = PromptGenerator.Generate(mode, criterion, depth, Translations.Current, files);

        _options!.ClassifyBtn.IsEnabled = false;
        ShowStatus(Translations.Get("Classifying"));
        try
        {
            string text = await _aiClient.GenerateAsync(_byok.ActiveProvider!, prompt);
            var results = ResponseParser.Parse(text, files, Translations.Current);
            if (results.Count == 0)
            {
                MessageBox.Show(Translations.Get("NoValidCategories"), Translations.Get("Error"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SetResults(results);
        }
        catch (AiException ex)
        {
            MessageBox.Show(Translations.AiErrorMessage(ex, Translations.Current), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _options.ClassifyBtn.IsEnabled = true;
        }
    }

    private void SetResults(List<ClassificationResult> results)
    {
        _results = results;
        ShowClassificationTree();
        _window!.MainControl.UpdateCounters();
    }

    private void ResetResults()
    {
        _results = new List<ClassificationResult>();
        if (_window != null)
            _window.MainControl.FileListCustomContent = null;
    }

    private void ShowClassificationTree()
    {
        var root = new DockPanel();

        var header = new TextBlock
        {
            Text = string.Format(Translations.Get("CategoriasDetectadas"), _results.Sum(r => r.Files.Count)),
            FontWeight = FontWeights.SemiBold,
            Foreground = System.Windows.Media.Brushes.Transparent,
            Margin = new Thickness(0, 0, 0, 8)
        };
        header.Foreground = (System.Windows.Media.Brush)FindResource("TextPrimaryBrush");
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var tree = new TreeView
        {
            Background = (System.Windows.Media.Brush)FindResource("SurfaceBrush"),
            BorderThickness = new Thickness(0)
        };
        foreach (var r in _results.OrderByDescending(r => r.Files.Count))
        {
            var catNode = new TreeViewItem
            {
                Header = $"{r.Category}  ({r.Files.Count})",
                IsExpanded = true,
                FontWeight = FontWeights.SemiBold
            };
            foreach (var file in r.Files)
                catNode.Items.Add(new TreeViewItem { Header = file, FontWeight = FontWeights.Normal });
            tree.Items.Add(catNode);
        }
        root.Children.Add(tree);

        _window!.MainControl.FileListCustomContent = root;
    }

    // ── Organizar ─────────────────────────────────────────────────────

    private async void OnActionClicked(object sender, RoutedEventArgs e)
    {
        if (_organizing)
        {
            _organizeCts?.Cancel();
            return;
        }

        if (_results.Count == 0)
        {
            MessageBox.Show(Translations.Get("NoResultsToOrganize"), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string output = _window!.MainControl.OutputFolderText;
        if (string.IsNullOrEmpty(output) || output == Translations.Get("OutputFolderDefault"))
        {
            MessageBox.Show(Translations.Get("ChooseOutputFolder"), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        bool copy = _options!.CopyRadio.IsChecked == true;
        int total = _results.Sum(r => r.Files.Count);

        if (MessageBox.Show(string.Format(Translations.Get("OrganizeConfirm"), total), Translations.Get("OrganizeResultTitle"),
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        _organizing = true;
        _organizeTotal = total;
        _window!.MainControl.ActionButtonText = Translations.Get("Cancel");
        var cts = new CancellationTokenSource();
        _organizeCts = cts;
        int processed = 0, errors = 0;

        var dupes = _window.Files.GroupBy(f => f.FileName, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (dupes.Count > 0)
        {
            MessageBox.Show(
                string.Format(Translations.Get("DuplicateFileNames"), string.Join(", ", dupes)),
                Translations.Get("Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var filePaths = _window.Files
            .ToDictionary(f => f.FileName, f => f.FilePath, StringComparer.OrdinalIgnoreCase);

        foreach (var item in _window.Files)
            item.Status = FileStatus.Queued;
        _window.MainControl.UpdateCounters();

        try
        {
            await Task.Run(() => FileOrganizer.Organize(
                filePaths, output, _results, copy, cts.Token,
                onProgress: (p, er) =>
                {
                    processed = p; errors = er;
                    _window?.Dispatcher.BeginInvoke(new Action(() => UpdateStatusBar(processed, errors, total)));
                },
                onFile: (name, ok) =>
                {
                    var item = _window!.Files.FirstOrDefault(f => f.FileName.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (item != null)
                        _window.Dispatcher.BeginInvoke(new Action(() => item.Status = ok ? FileStatus.Processed : FileStatus.Error));
                }));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Translations.Get("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            cts.Dispose();
            _organizeCts = null;
            _organizing = false;
            UpdateStatusBar(processed, errors, total);
            _window!.MainControl.ActionButtonText = Translations.Get("ActionButton");
            _window.MainControl.UpdateCounters();
        }

        if (cts.IsCancellationRequested)
        {
            MessageBox.Show(Translations.Get("Cancelled"), Translations.Get("OrganizeResultTitle"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            string msg = $"{Translations.Get("FilesOrganized")}\n\n{Translations.Get("ResultProcessed")}: {processed}\n{Translations.Get("ResultErrors")}: {errors}";
            MessageBox.Show(msg, Translations.Get("OrganizeResultTitle"), MessageBoxButton.OK,
                errors > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
    }

    private void UpdateStatusBar(int processed, int errors, int total)
    {
        _window!.MainControl.StatusBar.TotalCount = total;
        _window.MainControl.StatusBar.ProcessedCount = processed;
        _window.MainControl.StatusBar.ErrorCount = errors;
        _window.MainControl.StatusBar.PendingCount = Math.Max(0, total - processed - errors);
    }

    // ── BYOK ──────────────────────────────────────────────────────────

    private void OpenByokDialog()
    {
        var dialog = new ByokDialog(_byok, _byokStore, _aiClient, Translations.Current)
        {
            Owner = _window
        };
        dialog.ShowDialog();
    }

    // ── Acerca de ─────────────────────────────────────────────────────

    private void ShowAboutDialog()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly()?.GetName().Version;
        var about = new AboutDialog
        {
            AppName = Translations.Get("AppTitle"),
            AppVersion = $"v{version?.Major}.{version?.Minor}.{version?.Build ?? 0}",
            AppLogo = LoadIcon(),
            AppCopyright = "Clasificador IA",
            DialogTitle = Translations.Get("AboutButton"),
            ThirdPartyHeader = "Licenses",
            ThirdPartyLicenses = LoadThirdPartyNotices(),
            CheckUpdatesText = Translations.Get("AboutButton"),
            CloseButtonText = Translations.Get("CloseBtn", Translations.Current),
            Owner = _window
        };
        about.CheckUpdatesClick += (_, _) =>
        {
            MessageBox.Show(Translations.Get("AppUpToDate"), Translations.Get("AboutButton"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        };
        about.ShowDialog();
    }

    private string LoadThirdPartyNotices()
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "THIRD_PARTY_NOTICES.txt");
        try { return File.Exists(path) ? File.ReadAllText(path) : ""; }
        catch { return ""; }
    }

    // ── Estado ────────────────────────────────────────────────────────

    private void ShowStatus(string message)
    {
        if (_options != null)
            _options.PanelHintText.Text = message;
    }
}