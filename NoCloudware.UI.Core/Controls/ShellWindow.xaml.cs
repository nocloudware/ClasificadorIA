using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NoCloudware.UI.Core.ViewModels;

namespace NoCloudware.UI.Core.Controls;

public partial class ShellWindow : Window
{
    // ── Dependency Properties ─────────────────────────────────────────

    public static readonly DependencyProperty DropTextProperty =
        DependencyProperty.Register(nameof(DropText), typeof(string), typeof(ShellWindow),
            new PropertyMetadata("Drag and drop files here or click to select"));

    public static readonly DependencyProperty AcceptedFormatsTextProperty =
        DependencyProperty.Register(nameof(AcceptedFormatsText), typeof(string), typeof(ShellWindow),
            new PropertyMetadata(""));

    public static readonly DependencyProperty ActionButtonTextProperty =
        DependencyProperty.Register(nameof(ActionButtonText), typeof(string), typeof(ShellWindow),
            new PropertyMetadata("Action"));

    public static readonly DependencyProperty AboutButtonTextProperty =
        DependencyProperty.Register(nameof(AboutButtonText), typeof(string), typeof(ShellWindow),
            new PropertyMetadata("About"));

    public static readonly DependencyProperty DonateButtonTextProperty =
        DependencyProperty.Register(nameof(DonateButtonText), typeof(string), typeof(ShellWindow),
            new PropertyMetadata("Donate"));

    public static readonly DependencyProperty ExitButtonTextProperty =
        DependencyProperty.Register(nameof(ExitButtonText), typeof(string), typeof(ShellWindow),
            new PropertyMetadata("Exit"));

    // ── New parameterization DPs ──────────────────────────────────────

    public static readonly DependencyProperty FileListHeaderProperty =
        DependencyProperty.Register(nameof(FileListHeader), typeof(string), typeof(ShellWindow),
            new PropertyMetadata("Selected files"));

    public static readonly DependencyProperty OutputFolderTextProperty =
        DependencyProperty.Register(nameof(OutputFolderText), typeof(string), typeof(ShellWindow),
            new PropertyMetadata("Same folder as source"));

    public static readonly DependencyProperty WindowWidthProperty =
        DependencyProperty.Register(nameof(WindowWidth), typeof(double), typeof(ShellWindow),
            new PropertyMetadata(800.0));

    public static readonly DependencyProperty WindowHeightProperty =
        DependencyProperty.Register(nameof(WindowHeight), typeof(double), typeof(ShellWindow),
            new PropertyMetadata(720.0));

    public static readonly DependencyProperty WindowMinWidthProperty =
        DependencyProperty.Register(nameof(WindowMinWidth), typeof(double), typeof(ShellWindow),
            new PropertyMetadata(720.0));

    public static readonly DependencyProperty WindowMinHeightProperty =
        DependencyProperty.Register(nameof(WindowMinHeight), typeof(double), typeof(ShellWindow),
            new PropertyMetadata(640.0));

    public static readonly DependencyProperty WindowIconProperty =
        DependencyProperty.Register(nameof(WindowIcon), typeof(ImageSource), typeof(ShellWindow),
            new PropertyMetadata(null));

    // ── Events ────────────────────────────────────────────────────────

    public static readonly RoutedEvent AboutClickEvent =
        EventManager.RegisterRoutedEvent(nameof(AboutClick), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ShellWindow));

    public static readonly RoutedEvent DonateClickEvent =
        EventManager.RegisterRoutedEvent(nameof(DonateClick), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ShellWindow));

    public static readonly RoutedEvent ActionClickEvent =
        EventManager.RegisterRoutedEvent(nameof(ActionClick), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ShellWindow));

    public static readonly RoutedEvent ExitClickEvent =
        EventManager.RegisterRoutedEvent(nameof(ExitClick), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ShellWindow));

    public static readonly RoutedEvent FilesDroppedEvent =
        EventManager.RegisterRoutedEvent(nameof(FilesDropped), RoutingStrategy.Bubble,
            typeof(DropZone.FilesDroppedEventHandler), typeof(ShellWindow));

    // ── Properties ────────────────────────────────────────────────────

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

    public string ActionButtonText
    {
        get => (string)GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
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

    public string ExitButtonText
    {
        get => (string)GetValue(ExitButtonTextProperty);
        set => SetValue(ExitButtonTextProperty, value);
    }

    // ── New parameterization properties ───────────────────────────────

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

    public double WindowWidth
    {
        get => (double)GetValue(WindowWidthProperty);
        set => SetValue(WindowWidthProperty, value);
    }

    public double WindowHeight
    {
        get => (double)GetValue(WindowHeightProperty);
        set => SetValue(WindowHeightProperty, value);
    }

    public double WindowMinWidth
    {
        get => (double)GetValue(WindowMinWidthProperty);
        set => SetValue(WindowMinWidthProperty, value);
    }

    public double WindowMinHeight
    {
        get => (double)GetValue(WindowMinHeightProperty);
        set => SetValue(WindowMinHeightProperty, value);
    }

    public ImageSource? WindowIcon
    {
        get => (ImageSource?)GetValue(WindowIconProperty);
        set => SetValue(WindowIconProperty, value);
    }

    // ── Event wrappers ────────────────────────────────────────────────

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

    // ── Exposed controls & collections ────────────────────────────────

    public BaseMainControl MainControl => MainShell;
    public ObservableCollection<BaseFileItem> Files => MainShell.Files;
    public LanguageSelector LanguageSelector => MainShell.LanguageSelector;
    public ThemeToggle ThemeToggle => MainShell.ThemeToggle;
    public FileListBox FileListBox => MainShell.FileListBox;
    public StatusBar StatusBar => MainShell.StatusBar;

    // ── Constructor ───────────────────────────────────────────────────

    public ShellWindow()
    {
        InitializeComponent();

        AboutClick += (_, _) => { };
        DonateClick += (_, _) => { };
        ActionClick += (_, _) => { };
        ExitClick += (_, _) => { };
        FilesDropped += (_, _) => { };

        MainShell.AboutClick += (s, e) => RaiseEvent(new RoutedEventArgs(AboutClickEvent));
        MainShell.DonateClick += (s, e) => RaiseEvent(new RoutedEventArgs(DonateClickEvent));
        MainShell.ActionClick += (s, e) => RaiseEvent(new RoutedEventArgs(ActionClickEvent));
        MainShell.ExitClick += (s, e) => RaiseEvent(new RoutedEventArgs(ExitClickEvent));
        MainShell.FilesDropped += (s, e) =>
            RaiseEvent(new FilesDroppedEventArgs(FilesDroppedEvent, this, e.FilePaths));

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateBackgroundFromResources();
    }

    public void UpdateBackgroundFromResources()
    {
        if (Application.Current?.Resources["WindowBackgroundBrush"] is Brush brush)
            Background = brush;
    }

    // ── Internal handlers ─────────────────────────────────────────────

    private void OnShellHeaderDrag(object sender, RoutedEventArgs e)
    {
        if (Mouse.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void OnShellFilesDropped(object sender, FilesDroppedEventArgs e)
    {
    }
}
