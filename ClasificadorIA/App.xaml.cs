using System.Collections.Specialized;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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
    private CancellationTokenSource? _organizeCts;
    private CancellationTokenSource? _classifyCts;
    private readonly DispatcherTimer _batchTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private DateTime _batchStart;
    private int _batchIndex;
    private int _batchCount;
    private bool _organizing;
    private int _organizeTotal;

    private readonly List<BaseFileItem> _masterFiles = new();
    private readonly Dictionary<string, DateTime> _fileDates = new();
    private bool _rebuilding;

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
        _window.MainControl.FileListFooterContent.Content = CreateAddFolderButton();
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
            Idioma lang = LanguageHelpers.FromCulture(s.TryGetProperty("Language", out var l) ? l.GetString() : null);
            string? output = s.TryGetProperty("DefaultOutputPath", out var o) ? o.GetString() : null;
            return (dark, lang, output);
        }
        catch { return (true, Idioma.Español, null); }
    }

    private void SavePreferences()
    {
        try
        {
            string lang = Translations.Current.CultureCode();
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
        _window.MainControl.OptionsPanelMinWidth = 300;
        _window.MainControl.OutputFolderText = !string.IsNullOrEmpty(savedOutput)
            ? savedOutput
            : Translations.Get("OutputFolderDefault");

        var flagBase = "pack://application:,,,/NoCloudware.UI.Core;component/Assets/Flags/";
        _window.LanguageSelector.Languages = new System.Collections.ObjectModel.ObservableCollection<LanguageItem>
        {
            new("Español",   $"{flagBase}flag-es.png", "es"),
            new("English",   $"{flagBase}flag-uk.png", "en"),
            new("Français",  $"{flagBase}flag-fr.png", "fr"),
            new("Deutsch",   $"{flagBase}flag-de.png", "de"),
            new("Português", $"{flagBase}flag-br.png", "pt"),
            new("Italiano",  $"{flagBase}flag-it.png", "it"),
            new("日本語",     $"{flagBase}flag-jp.png", "ja"),
            new("中文",       $"{flagBase}flag-cn.png", "zh"),
        };
        _window.LanguageSelector.ComboMaxWidth = 36;
        _window.LanguageSelector.SetLanguage(Translations.Current.CultureCode());
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
        _window.MainControl.OutputFolderLabel = Translations.Get("OutputFolder");

        _window.MainControl.StatusBar.TotalLabel = Translations.Get("StatusTotal");
        _window.MainControl.StatusBar.ProcessedLabel = Translations.Get("StatusProcessed");
        _window.MainControl.StatusBar.PendingLabel = Translations.Get("StatusPending");
        _window.MainControl.StatusBar.ErrorsLabel = Translations.Get("StatusErrors");

        _window.FileListBox.RemoveMenuItemText = Translations.Get("Remove");
        _window.FileListBox.ClearAllMenuItemText = Translations.Get("ClearAll");

        if (string.IsNullOrEmpty(_window.MainControl.OutputFolderText) ||
            _window.MainControl.OutputFolderText.Equals("Same folder as source", StringComparison.OrdinalIgnoreCase) ||
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

        _window.Files.CollectionChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (var old in e.OldItems)
                    if (old is BaseFileItem f)
                        _masterFiles.RemoveAll(m => m.FilePath.Equals(f.FilePath, StringComparison.OrdinalIgnoreCase));
                ResetResults();
                _window!.MainControl.UpdateCounters();
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset && !_rebuilding)
            {
                _masterFiles.Clear();
                ResetResults();
                _window!.MainControl.UpdateCounters();
            }
        };

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
                Translations.Current = LanguageHelpers.FromCulture(a.CultureCode);
                ApplyLanguage();
                SavePreferences();
            }
        };

        _window.DonateClick += (_, _) => new DonationService("https://nocloudware.com/donate.html").OpenDonationPage();

        _window.AboutClick += (_, _) => ShowAboutDialog();

        _window.Closing += (_, _) =>
        {
            _organizeCts?.Cancel();
            _classifyCts?.Cancel();
            SavePreferences();
        };

        _batchTimer.Tick += (_, _) => ShowBatchTimer();

        _options!.ClassifyBtn.Click += (_, _) => _ = ClassifyAsync();
        _options.ByokButton.Click += (_, _) => OpenByokDialog();
        _options.CancelClassifyLink.Click += (_, _) => _classifyCts?.Cancel();

        _options.ModeCombo.SelectionChanged += (_, _) => { ResetResults(); RefreshMetadata(); };
        _options.CriterionCombo.SelectionChanged += (_, _) => ResetResults();
        _options.DepthCombo.SelectionChanged += (_, _) => ResetResults();

        _options.DedupMasterCheck.Checked += (_, _) => ApplyDedupFilter();
        _options.DedupMasterCheck.Unchecked += (_, _) => ApplyDedupFilter();
        _options.SizeCheck.Checked += (_, _) => ApplyDedupFilter();
        _options.SizeCheck.Unchecked += (_, _) => ApplyDedupFilter();
        _options.SizeMenorRadio.Checked += (_, _) => ApplyDedupFilter();
        _options.SizeMayorRadio.Checked += (_, _) => ApplyDedupFilter();
        _options.DateCheck.Checked += (_, _) => ApplyDedupFilter();
        _options.DateCheck.Unchecked += (_, _) => ApplyDedupFilter();
        _options.DateMenorRadio.Checked += (_, _) => ApplyDedupFilter();
        _options.DateMayorRadio.Checked += (_, _) => ApplyDedupFilter();
    }

    // ── Archivos ──────────────────────────────────────────────────────

    private void AddFiles(IEnumerable<string> paths)
    {
        var nowFiles = _window!.Files;
        foreach (var item in nowFiles)
            if (!_masterFiles.Any(f => f.FilePath.Equals(item.FilePath, StringComparison.OrdinalIgnoreCase)))
                _masterFiles.Add(item);

        foreach (var path in paths)
        {
            if (FileFilters.IsSystemFile(Path.GetFileName(path))) continue;
            if (_masterFiles.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase))) continue;
            _masterFiles.Add(new BaseFileItem
            {
                FilePath = path,
                FileName = Path.GetFileName(path),
                FileSize = new FileInfo(path).Length
            });
        }
ApplyDedupFilter();
        RefreshMetadata();
    }

    private void RefreshMetadata()
    {
        var (mode, _, _) = GetOptions();
        var idioma = Translations.Current;
        _window!.FileListBox.SetMetadataHeaders(
            string.Equals(mode.Key, "Genérico", StringComparison.OrdinalIgnoreCase)
                ? Array.Empty<string>()
                : ClassificationModes.GetCriteria(mode, idioma));
        foreach (var item in _window.Files)
            item.MetadataCells = MetadataClassifier.GetCells(item.FilePath, mode, idioma);
    }

    private DateTime GetFileDate(string path)
    {
        if (!_fileDates.TryGetValue(path, out var date))
        {
            date = File.GetLastWriteTime(path);
            _fileDates[path] = date;
        }
        return date;
    }

    private void ApplyDedupFilter()
    {
        var visible = DedupFilter.Keep(_masterFiles,
            f => f.FileName, f => f.FileSize, f => GetFileDate(f.FilePath),
            _options!.SameNameChecked, _options.SizeEnabled, !_options.SizeMenor,
            _options.DateEnabled, !_options.DateMenor);

        _rebuilding = true;
        try
        {
            _window!.Files.Clear();
            foreach (var item in visible.OrderBy(f => f.FileName, StringComparer.OrdinalIgnoreCase))
                _window.Files.Add(item);
        }
        finally
        {
            _rebuilding = false;
        }

        ResetResults();
        _window.MainControl.UpdateCounters();
    }

    private AddFolderButton CreateAddFolderButton()
    {
        var button = new AddFolderButton();
        button.Clicked += (_, _) => OpenSourceFolder();
        return button;
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

        var added = Directory.GetFiles(dialog.FolderName, "*", SearchOption.AllDirectories)
            .Where(f => !FileFilters.IsSystemFile(Path.GetFileName(f)))
            .ToArray();
        AddFiles(added);
    }

    // ── Clasificación ─────────────────────────────────────────────────

    private (ClassificationMode mode, string criterion, int depth) GetOptions()
    {
        string? modeKey = (_options?.ModeCombo.SelectedItem as ComboBoxItem)?.Tag as string;
        string criterion = (_options?.CriterionCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "Tema";
        int depth = (_options?.DepthCombo.SelectedItem as ComboBoxItem)?.Tag is int d ? d : 5;
        return (ClassificationModes.Find(modeKey ?? "") ?? ClassificationModes.Default, criterion, depth);
    }

    private string[] GetFileNames() =>
        _window!.Files.Where(f => !FileFilters.IsSystemFile(f.FileName)).Select(f => f.FileName).ToArray();

    private string[] GetFilePaths() =>
        _window!.Files.Where(f => !FileFilters.IsSystemFile(f.FileName)).Select(f => f.FilePath).ToArray();

    private async Task ClassifyAsync()
    {
        var files = GetFileNames();
        if (files.Length == 0)
        {
            MessageBox.Show(Translations.Get("NoFilesToClassify"), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_options!.MethodIa.IsChecked == true)
            await ClassifyIa(files);
        else
            ClassifyLocal(GetFilePaths());
    }

    private void ClassifyLocal(string[] paths)
    {
        var (_, criterion, depth) = GetOptions();
        var results = LocalClassifier.Classify(paths, depth, Translations.Current, criterion);
        if (results.Count == 0)
        {
            MessageBox.Show(Translations.Get("NoValidCategories"), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        SetResults(results);
    }

    private async Task ClassifyIa(string[] files)
    {
        var provider = _byok.ActiveProvider;
        if (provider == null || _byok.Providers.Count == 0)
        {
            OpenByokDialog();
            if (_byok.ActiveProvider == null) return;
        }

        var (mode, criterion, depth) = GetOptions();
        int batchSize = _options!.BatchSize;

        _options!.ClassifyBtn.IsEnabled = false;
        var cts = new CancellationTokenSource();
        _classifyCts = cts;
        _options.TimerText.Visibility = Visibility.Visible;
        _options.CancelClassifyLink.Visibility = Visibility.Visible;
        try
        {
            var active = _byok.ActiveProvider!;
            active.MaxOutputLimit = ModelLimits.Resolve(active, active.SelectedModel);
            var classifier = new BatchClassifier(
                (prompt, budget) => _aiClient.GenerateAsync(active, prompt, budget, cts.Token),
                batchSize, depth, active.MaxOutputLimit);
            var results = await classifier.ClassifyAsync(
                mode, criterion, Translations.Current, files, ShowStatus,
                onBatchStarted: (i, n) => StartBatchTimer(i, n), cts.Token);
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
        catch (OperationCanceledException)
        {
            MessageBox.Show(Translations.Get("ClassifyCancelled"), Translations.Get("Error"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        finally
        {
            _batchTimer.Stop();
            _options!.TimerText.Visibility = Visibility.Collapsed;
            _options.CancelClassifyLink.Visibility = Visibility.Collapsed;
            _options.ClassifyBtn.IsEnabled = true;
            cts.Dispose();
            _classifyCts = null;
        }
    }

    // Cronómetro por lote: arranca en 0 al comenzar cada lote y avanza 1s/s.
    private void StartBatchTimer(int index, int count)
    {
        _batchIndex = index;
        _batchCount = count;
        _batchStart = DateTime.Now;
        _batchTimer.Stop();
        _batchTimer.Start();
        ShowBatchTimer();
    }

    private void ShowBatchTimer()
    {
        if (_options == null) return;
        var elapsed = DateTime.Now - _batchStart;
        string text = string.Format(Translations.Get("BatchTimer"),
            _batchIndex, _batchCount, elapsed.ToString(@"mm\:ss"));
        void Set() => _options.TimerText.Text = text;
        if (_options.Dispatcher.CheckAccess())
            Set();
        else
            _options.Dispatcher.BeginInvoke(Set);
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
            Text = string.Format(Translations.Get("CategoriasDetectadas"), _results.Count),
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
                IsExpanded = false,
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
        var ci = Translations.Current;
        var assemblyVer = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
        var appVersion = assemblyVer != null
            ? $"{assemblyVer.Major}.{assemblyVer.Minor}.{assemblyVer.Build}"
            : "1.0.0";

        var wpfUiVersion = GetAssemblyVersion("Wpf.Ui");
        var mvvmVersion = GetAssemblyVersion("CommunityToolkit.Mvvm");

        var about = new AboutDialog
        {
            AppName = Translations.Get("AppTitle", ci),
            AppVersion = $"{Translations.Get("AboutVersion", ci)} v{appVersion}",
            AppLogo = LoadIcon(),
            DialogTitle = Translations.Get("AboutTitle", ci),
            CreditsHeader = Translations.Get("AboutCredits", ci),
            DevelopedByText = Translations.Get("AboutDevelopedBy", ci),
            DeveloperName = Translations.Get("AboutDeveloperName", ci),
            DeveloperUrl = "https://www.nocloudware.com",
            ThirdPartyLibrariesText = Translations.Get("AboutThirdPartyLibraries", ci),
            WpfUiDesc = Translations.Get("AboutWpfUiDesc", ci) + (wpfUiVersion != null ? $" (v{wpfUiVersion})" : ""),
            MvvmDesc = Translations.Get("AboutMvvmDesc", ci) + (mvvmVersion != null ? $" (v{mvvmVersion})" : ""),
            SpecialThanksText = Translations.Get("AboutSpecialThanks", ci),
            SpecialThanksMessage = Translations.Get("AboutSpecialThanksMessage", ci),
            TechnologiesUsedText = Translations.Get("AboutTechnologiesUsed", ci),
            TechList = Translations.Get("AboutTechList", ci),
            LicenseText = Translations.Get("AboutLicense", ci),
            LicenseInfo = Translations.Get("AboutLicenseInfo", ci),
            CheckUpdatesText = Translations.Get("CheckUpdatesBtn", ci),
            CloseButtonText = Translations.Get("CloseBtn", ci),
            Owner = _window
        };
        about.CheckUpdatesClick += async (_, _) =>
        {
            var button = about.FindName("CheckUpdatesButton") as System.Windows.Controls.Button;
            if (button != null) button.IsEnabled = false;
            var check = await new UpdateService("nocloudware", "ClasificadorIA").CheckForUpdatesAsync(appVersion);
            if (check != null && check.IsNewerVersion)
                MessageBox.Show(string.Format(Translations.Get("UpdateAvailable", ci), check.Version, check.DownloadUrl),
                    Translations.Get("AboutTitle", ci), MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(Translations.Get("AppUpToDate"), Translations.Get("AboutTitle", ci),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            if (button != null) button.IsEnabled = true;
        };
        about.ShowDialog();
    }

    private static string? GetAssemblyVersion(string assemblyName)
    {
        try
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);
            asm ??= System.Reflection.Assembly.Load(assemblyName);
            var v = asm.GetName().Version;
            if (v == null) return null;
            return v.Build != 0 ? $"{v.Major}.{v.Minor}.{v.Build}" : $"{v.Major}.{v.Minor}";
        }
        catch { return null; }
    }

    // ── Estado ────────────────────────────────────────────────────────

    private void ShowStatus(string message)
    {
        if (_options == null) return;
        void Set() => _options.PanelHintText.Text = message;
        if (_options.Dispatcher.CheckAccess())
            Set();
        else
            _options.Dispatcher.BeginInvoke(Set);
    }
}