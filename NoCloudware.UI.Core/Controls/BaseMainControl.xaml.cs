using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using NoCloudware.UI.Core.ViewModels;

namespace NoCloudware.UI.Core.Controls;

public partial class BaseMainControl : UserControl
{
    // ── Dependency Properties ─────────────────────────────────────────

    public static readonly DependencyProperty AppTitleProperty =
        DependencyProperty.Register(nameof(AppTitle), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("", OnHeaderChanged));

    public static readonly DependencyProperty AppTaglineProperty =
        DependencyProperty.Register(nameof(AppTagline), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("", OnHeaderChanged));

    public static readonly DependencyProperty AppLogoProperty =
        DependencyProperty.Register(nameof(AppLogo), typeof(ImageSource), typeof(BaseMainControl),
            new PropertyMetadata(null, OnHeaderChanged));

    public static readonly DependencyProperty DropTextProperty =
        DependencyProperty.Register(nameof(DropText), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata(""));

    public static readonly DependencyProperty AcceptedFormatsTextProperty =
        DependencyProperty.Register(nameof(AcceptedFormatsText), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata(""));

    public static readonly DependencyProperty FileListHeaderProperty =
        DependencyProperty.Register(nameof(FileListHeader), typeof(string), typeof(BaseMainControl),
            new FrameworkPropertyMetadata("Selected files", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty OutputFolderTextProperty =
        DependencyProperty.Register(nameof(OutputFolderText), typeof(string), typeof(BaseMainControl),
            new FrameworkPropertyMetadata("Same folder as source", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty AboutButtonTextProperty =
        DependencyProperty.Register(nameof(AboutButtonText), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("About"));

    public static readonly DependencyProperty DonateButtonTextProperty =
        DependencyProperty.Register(nameof(DonateButtonText), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("Donate"));

    public static readonly DependencyProperty ActionButtonTextProperty =
        DependencyProperty.Register(nameof(ActionButtonText), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("Action"));

    public static readonly DependencyProperty ExitButtonTextProperty =
        DependencyProperty.Register(nameof(ExitButtonText), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("Exit"));

    public static readonly DependencyProperty SelectFilesButtonTextProperty =
        DependencyProperty.Register(nameof(SelectFilesButtonText), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("Select files"));

    public static readonly DependencyProperty ChangeButtonTextProperty =
        DependencyProperty.Register(nameof(ChangeButtonText), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("Change"));

    // ── New parameterization DPs ──────────────────────────────────────

    public static readonly DependencyProperty HeaderBackgroundProperty =
        DependencyProperty.Register(nameof(HeaderBackground), typeof(Brush), typeof(BaseMainControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderOverlayOpacityProperty =
        DependencyProperty.Register(nameof(HeaderOverlayOpacity), typeof(double), typeof(BaseMainControl),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty LogoWidthProperty =
        DependencyProperty.Register(nameof(LogoWidth), typeof(double), typeof(BaseMainControl),
            new PropertyMetadata(20.0, OnHeaderChanged));

    public static readonly DependencyProperty LogoHeightProperty =
        DependencyProperty.Register(nameof(LogoHeight), typeof(double), typeof(BaseMainControl),
            new PropertyMetadata(20.0, OnHeaderChanged));

    public static readonly DependencyProperty LogoMarginProperty =
        DependencyProperty.Register(nameof(LogoMargin), typeof(Thickness), typeof(BaseMainControl),
            new PropertyMetadata(new Thickness(0, 0, 7, 0), OnHeaderChanged));

    public static readonly DependencyProperty AppTitleFontSizeProperty =
        DependencyProperty.Register(nameof(AppTitleFontSize), typeof(double), typeof(BaseMainControl),
            new PropertyMetadata(20.0, OnHeaderChanged));

    public static readonly DependencyProperty TaglineFontSizeProperty =
        DependencyProperty.Register(nameof(TaglineFontSize), typeof(double), typeof(BaseMainControl),
            new PropertyMetadata(12.0, OnHeaderChanged));

    public static readonly DependencyProperty FileDialogFilterProperty =
        DependencyProperty.Register(nameof(FileDialogFilter), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("All files (*.*)|*.*"));

    public static readonly DependencyProperty OutputFolderDialogTitleProperty =
        DependencyProperty.Register(nameof(OutputFolderDialogTitle), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("Select output folder"));

    public static readonly DependencyProperty OptionsPanelMinWidthProperty =
        DependencyProperty.Register(nameof(OptionsPanelMinWidth), typeof(double), typeof(BaseMainControl),
            new PropertyMetadata(200.0));

    public static readonly DependencyProperty OptionsPanelPaddingProperty =
        DependencyProperty.Register(nameof(OptionsPanelPadding), typeof(Thickness), typeof(BaseMainControl),
            new PropertyMetadata(new Thickness(12)));

    public static readonly DependencyProperty ActionButtonMinWidthProperty =
        DependencyProperty.Register(nameof(ActionButtonMinWidth), typeof(double), typeof(BaseMainControl),
            new PropertyMetadata(100.0));

    public static readonly DependencyProperty FileCountFormatProperty =
        DependencyProperty.Register(nameof(FileCountFormat), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata("({0})"));

    public static readonly DependencyProperty FileListCustomContentProperty =
        DependencyProperty.Register(nameof(FileListCustomContent), typeof(object), typeof(BaseMainControl),
            new PropertyMetadata(null, OnFileListCustomContentChanged));

    // ── Banner DPs ────────────────────────────────────────────────────

    public static readonly DependencyProperty BannerBitmapProperty =
        DependencyProperty.Register(nameof(BannerBitmap), typeof(ImageSource), typeof(BaseMainControl),
            new PropertyMetadata(null, OnBannerChanged));

    public static readonly DependencyProperty BannerHeightProperty =
        DependencyProperty.Register(nameof(BannerHeight), typeof(double), typeof(BaseMainControl),
            new PropertyMetadata(64.0));

    public static readonly DependencyProperty BannerVisibleProperty =
        DependencyProperty.Register(nameof(BannerVisible), typeof(Visibility), typeof(BaseMainControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty BannerTextVisibleProperty =
        DependencyProperty.Register(nameof(BannerTextVisible), typeof(Visibility), typeof(BaseMainControl),
            new PropertyMetadata(Visibility.Visible));

    // ── Bottom-bar tips DP ────────────────────────────────────────────

    public static readonly DependencyProperty TipsTextProperty =
        DependencyProperty.Register(nameof(TipsText), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata(""));

    public static readonly DependencyProperty TipsFontSizeProperty =
        DependencyProperty.Register(nameof(TipsFontSize), typeof(double), typeof(BaseMainControl),
            new PropertyMetadata(12.0));

    public static readonly DependencyProperty TipsForegroundProperty =
        DependencyProperty.Register(nameof(TipsForeground), typeof(Brush), typeof(BaseMainControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CurrentTipProperty =
        DependencyProperty.Register(nameof(CurrentTip), typeof(string), typeof(BaseMainControl),
            new PropertyMetadata(""));

    public static readonly DependencyProperty TipsListProperty =
        DependencyProperty.Register(nameof(TipsList), typeof(string[]), typeof(BaseMainControl),
            new PropertyMetadata(null, OnTipsListChanged));

    private int _tipsIndex;

    // ── Donate visibility DP ──────────────────────────────────────────

    public static readonly DependencyProperty DonateButtonVisibleProperty =
        DependencyProperty.Register(nameof(DonateButtonVisible), typeof(Visibility), typeof(BaseMainControl),
            new PropertyMetadata(Visibility.Visible));

    // ── Read-only exposed controls ────────────────────────────────────

    public LanguageSelector LanguageSelector => LangSelector;
    public ThemeToggle ThemeToggle => ThemeToggler;
    public FileListBox FileListBox => FileList;
    public StatusBar StatusBar => StatusBarControl;
    public ContentControl OptionsContent => OptionsPanel;
    public ContentControl FileListFooterContent => FileListFooterPanel;
    public ObservableCollection<BaseFileItem> Files { get; } = new();

    // ── Events ────────────────────────────────────────────────────────

    public static readonly RoutedEvent AboutClickEvent =
        EventManager.RegisterRoutedEvent(nameof(AboutClick), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(BaseMainControl));

    public static readonly RoutedEvent DonateClickEvent =
        EventManager.RegisterRoutedEvent(nameof(DonateClick), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(BaseMainControl));

    public static readonly RoutedEvent ActionClickEvent =
        EventManager.RegisterRoutedEvent(nameof(ActionClick), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(BaseMainControl));

    public static readonly RoutedEvent ExitClickEvent =
        EventManager.RegisterRoutedEvent(nameof(ExitClick), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(BaseMainControl));

    public static readonly RoutedEvent FilesDroppedEvent =
        EventManager.RegisterRoutedEvent(nameof(FilesDropped), RoutingStrategy.Bubble,
            typeof(DropZone.FilesDroppedEventHandler), typeof(BaseMainControl));

    public static readonly RoutedEvent OutputFolderChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(OutputFolderChanged), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(BaseMainControl));

    public static readonly RoutedEvent HeaderDragEvent =
        EventManager.RegisterRoutedEvent(nameof(HeaderDrag), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(BaseMainControl));

    public event RoutedEventHandler AboutClick
    {
        add => AddHandler(AboutClickEvent, value);
        remove => RemoveHandler(AboutClickEvent, value);
    }

    public event RoutedEventHandler DonateClick
    {
        add => AddHandler(DonateClickEvent, value);
        remove => RemoveHandler(DonateClickEvent, value);
    }

    public event RoutedEventHandler ActionClick
    {
        add => AddHandler(ActionClickEvent, value);
        remove => RemoveHandler(ActionClickEvent, value);
    }

    public event RoutedEventHandler ExitClick
    {
        add => AddHandler(ExitClickEvent, value);
        remove => RemoveHandler(ExitClickEvent, value);
    }

    public event DropZone.FilesDroppedEventHandler FilesDropped
    {
        add => AddHandler(FilesDroppedEvent, value);
        remove => RemoveHandler(FilesDroppedEvent, value);
    }

    public event RoutedEventHandler OutputFolderChanged
    {
        add => AddHandler(OutputFolderChangedEvent, value);
        remove => RemoveHandler(OutputFolderChangedEvent, value);
    }

    public event RoutedEventHandler HeaderDrag
    {
        add => AddHandler(HeaderDragEvent, value);
        remove => RemoveHandler(HeaderDragEvent, value);
    }

    // ── Properties ────────────────────────────────────────────────────

    public string AppTitle
    {
        get => (string)GetValue(AppTitleProperty);
        set => SetValue(AppTitleProperty, value);
    }

    public string AppTagline
    {
        get => (string)GetValue(AppTaglineProperty);
        set => SetValue(AppTaglineProperty, value);
    }

    public ImageSource? AppLogo
    {
        get => (ImageSource?)GetValue(AppLogoProperty);
        set => SetValue(AppLogoProperty, value);
    }

    public string DropText
    {
        get => (string)GetValue(DropTextProperty);
        set => SetValue(DropTextProperty, value);
    }

    public string AcceptedFormatsText
    {
        get => (string)GetValue(AcceptedFormatsTextProperty);
        set => SetValue(AcceptedFormatsTextProperty, value);
    }

    public string FileListHeader
    {
        get => (string)GetValue(FileListHeaderProperty);
        set => SetValue(FileListHeaderProperty, value);
    }

    public string OutputFolderText
    {
        get => (string)GetValue(OutputFolderTextProperty);
        set => SetValue(OutputFolderTextProperty, value);
    }

    public string AboutButtonText
    {
        get => (string)GetValue(AboutButtonTextProperty);
        set => SetValue(AboutButtonTextProperty, value);
    }

    public string DonateButtonText
    {
        get => (string)GetValue(DonateButtonTextProperty);
        set => SetValue(DonateButtonTextProperty, value);
    }

    public string ActionButtonText
    {
        get => (string)GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    public string ExitButtonText
    {
        get => (string)GetValue(ExitButtonTextProperty);
        set => SetValue(ExitButtonTextProperty, value);
    }

    public string SelectFilesButtonText
    {
        get => (string)GetValue(SelectFilesButtonTextProperty);
        set => SetValue(SelectFilesButtonTextProperty, value);
    }

    public string ChangeButtonText
    {
        get => (string)GetValue(ChangeButtonTextProperty);
        set => SetValue(ChangeButtonTextProperty, value);
    }

    // ── New parameterization properties ───────────────────────────────

    public Brush? HeaderBackground
    {
        get => (Brush?)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public double HeaderOverlayOpacity
    {
        get => (double)GetValue(HeaderOverlayOpacityProperty);
        set => SetValue(HeaderOverlayOpacityProperty, value);
    }

    public double LogoWidth
    {
        get => (double)GetValue(LogoWidthProperty);
        set => SetValue(LogoWidthProperty, value);
    }

    public double LogoHeight
    {
        get => (double)GetValue(LogoHeightProperty);
        set => SetValue(LogoHeightProperty, value);
    }

    public Thickness LogoMargin
    {
        get => (Thickness)GetValue(LogoMarginProperty);
        set => SetValue(LogoMarginProperty, value);
    }

    public double AppTitleFontSize
    {
        get => (double)GetValue(AppTitleFontSizeProperty);
        set => SetValue(AppTitleFontSizeProperty, value);
    }

    public double TaglineFontSize
    {
        get => (double)GetValue(TaglineFontSizeProperty);
        set => SetValue(TaglineFontSizeProperty, value);
    }

    public string FileDialogFilter
    {
        get => (string)GetValue(FileDialogFilterProperty);
        set => SetValue(FileDialogFilterProperty, value);
    }

    public string OutputFolderDialogTitle
    {
        get => (string)GetValue(OutputFolderDialogTitleProperty);
        set => SetValue(OutputFolderDialogTitleProperty, value);
    }

    public double OptionsPanelMinWidth
    {
        get => (double)GetValue(OptionsPanelMinWidthProperty);
        set => SetValue(OptionsPanelMinWidthProperty, value);
    }

    public Thickness OptionsPanelPadding
    {
        get => (Thickness)GetValue(OptionsPanelPaddingProperty);
        set => SetValue(OptionsPanelPaddingProperty, value);
    }

    public double ActionButtonMinWidth
    {
        get => (double)GetValue(ActionButtonMinWidthProperty);
        set => SetValue(ActionButtonMinWidthProperty, value);
    }

    public string FileCountFormat
    {
        get => (string)GetValue(FileCountFormatProperty);
        set => SetValue(FileCountFormatProperty, value);
    }

    public object? FileListCustomContent
    {
        get => GetValue(FileListCustomContentProperty);
        set => SetValue(FileListCustomContentProperty, value);
    }

    // ── Banner properties ─────────────────────────────────────────────

    public ImageSource? BannerBitmap
    {
        get => (ImageSource?)GetValue(BannerBitmapProperty);
        set => SetValue(BannerBitmapProperty, value);
    }

    public double BannerHeight
    {
        get => (double)GetValue(BannerHeightProperty);
        set => SetValue(BannerHeightProperty, value);
    }

    public Visibility BannerVisible
    {
        get => (Visibility)GetValue(BannerVisibleProperty);
        set => SetValue(BannerVisibleProperty, value);
    }

    public Visibility BannerTextVisible
    {
        get => (Visibility)GetValue(BannerTextVisibleProperty);
        set => SetValue(BannerTextVisibleProperty, value);
    }

    // ── Bottom-bar tips properties ────────────────────────────────────

    public string TipsText
    {
        get => (string)GetValue(TipsTextProperty);
        set => SetValue(TipsTextProperty, value);
    }

    public double TipsFontSize
    {
        get => (double)GetValue(TipsFontSizeProperty);
        set => SetValue(TipsFontSizeProperty, value);
    }

    public Brush? TipsForeground
    {
        get => (Brush?)GetValue(TipsForegroundProperty);
        set => SetValue(TipsForegroundProperty, value);
    }

    public string CurrentTip
    {
        get => (string)GetValue(CurrentTipProperty);
        set => SetValue(CurrentTipProperty, value);
    }

    public string[]? TipsList
    {
        get => (string[]?)GetValue(TipsListProperty);
        set => SetValue(TipsListProperty, value);
    }

    // ── Donate visibility ─────────────────────────────────────────────

    public Visibility DonateButtonVisible
    {
        get => (Visibility)GetValue(DonateButtonVisibleProperty);
        set => SetValue(DonateButtonVisibleProperty, value);
    }

    // ── Constructor ───────────────────────────────────────────────────

    public BaseMainControl()
    {
        InitializeComponent();
        FileList.Items = Files;
        UpdateHeader();
    }

    // ── Tips rotation ─────────────────────────────────────────────────

    private static void OnTipsListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BaseMainControl c)
            c.RotateTip(0);
    }

    private void RotateTip(int index)
    {
        var list = TipsList;
        if (list is { Length: > 0 })
        {
            _tipsIndex = (index + list.Length) % list.Length;
            CurrentTip = list[_tipsIndex];
        }
    }

    private void OnTipsClick(object sender, MouseButtonEventArgs e)
    {
        RotateTip(_tipsIndex + 1);
    }

    private static void OnBannerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BaseMainControl c)
            c.UpdateHeader();
    }

    private static void OnFileListCustomContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BaseMainControl c)
            c.UpdateFileListCustomContent();
    }

    private void UpdateFileListCustomContent()
    {
        var hasCustom = FileListCustomContent != null;
        if (FileListCustomPanel != null)
        {
            FileListCustomPanel.Content = FileListCustomContent;
            FileListCustomPanel.Visibility = hasCustom ? Visibility.Visible : Visibility.Collapsed;
        }
        if (FileList != null)
            FileList.Visibility = hasCustom ? Visibility.Collapsed : Visibility.Visible;
        UpdateCounters();
    }

    // ── Event handlers ────────────────────────────────────────────────

    private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Solo hacer drag si el click fue en el header, no en botones interactivos
        var source = e.OriginalSource as DependencyObject;
        if (source != null)
        {
            // Si el click fue en un Button, ToggleButton, o cualquier control interactivo, no hacer drag
            if (source is System.Windows.Controls.Button || 
                source is System.Windows.Controls.Primitives.ToggleButton ||
                source is System.Windows.Controls.TextBox ||
                source is System.Windows.Controls.PasswordBox ||
                source is System.Windows.Controls.ComboBox ||
                source is System.Windows.Controls.CheckBox)
                return;
            
            // Verificar si algún padre es un botón (por ejemplo, botones con contenido complejo)
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(source);
            while (parent != null)
            {
                if (parent is System.Windows.Controls.Button || 
                    parent is System.Windows.Controls.Primitives.ToggleButton)
                    return;
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
        }
        
        RaiseEvent(new RoutedEventArgs(HeaderDragEvent));
    }

    private void OnFilesDroppedCore(object sender, FilesDroppedEventArgs e)
    {
        var files = new List<string>();
        foreach (var path in e.FilePaths)
        {
            if (System.IO.Directory.Exists(path))
                files.AddRange(System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories));
            else if (System.IO.File.Exists(path))
                files.Add(path);
        }

        foreach (var path in files)
        {
            if (Files.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                continue;
            Files.Add(new BaseFileItem
            {
                FilePath = path,
                FileName = System.IO.Path.GetFileName(path),
                FileSize = new System.IO.FileInfo(path).Length
            });
        }
        UpdateCounters();
        RaiseEvent(new FilesDroppedEventArgs(FilesDroppedEvent, this, files.ToArray()));
    }

    private void OnSelectFiles(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = FileDialogFilter
        };
        if (dialog.ShowDialog() == true)
        {
            OnFilesDroppedCore(sender, new FilesDroppedEventArgs(FilesDroppedEvent, this, dialog.FileNames));
        }
    }

    private void OnLeftPanelDragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void OnLeftPanelDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
            e.Data.GetData(DataFormats.FileDrop) is string[] files)
        {
            OnFilesDroppedCore(sender, new FilesDroppedEventArgs(FilesDroppedEvent, this, files));
        }
    }

    private void OnSelectOutputFolder(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = OutputFolderDialogTitle,
            Multiselect = false
        };
        if (dialog.ShowDialog() == true)
        {
            OutputFolderText = dialog.FolderName;
            RaiseEvent(new RoutedEventArgs(OutputFolderChangedEvent));
        }
    }

    private void OnAboutClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(AboutClickEvent));
    }

    private void OnDonateClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(DonateClickEvent));
    }

    private void OnActionClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ActionClickEvent));
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ExitClickEvent));
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BaseMainControl c)
            c.UpdateHeader();
    }

    private void UpdateHeader()
    {
        if (AppTitleText != null)
            AppTitleText.Text = AppTitle;
    }

    public void UpdateCounters()
    {
        var total = Files.Count;
        var processed = 0;
        var pending = 0;
        var errors = 0;
        foreach (var f in Files)
        {
            switch (f.Status)
            {
                case FileStatus.Processed: processed++; break;
                case FileStatus.Queued:
                case FileStatus.Processing: pending++; break;
                case FileStatus.Error: errors++; break;
            }
        }

        StatusBarControl.TotalCount = total;
        StatusBarControl.ProcessedCount = processed;
        StatusBarControl.PendingCount = pending;
        StatusBarControl.ErrorCount = errors;

        FileCountText.Text = total > 0 ? string.Format(FileCountFormat, total) : "";
        EmptyListText.Visibility = total > 0 || FileListCustomContent != null ? Visibility.Collapsed : Visibility.Visible;
    }
}
