using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace NoCloudware.UI.Core.Controls;

public partial class DropZone : UserControl
{
    public static readonly RoutedEvent FilesDroppedEvent =
        EventManager.RegisterRoutedEvent(nameof(FilesDropped), RoutingStrategy.Bubble,
            typeof(FilesDroppedEventHandler), typeof(DropZone));

    public static readonly RoutedEvent FilesSelectedEvent =
        EventManager.RegisterRoutedEvent(nameof(FilesSelected), RoutingStrategy.Bubble,
            typeof(FilesDroppedEventHandler), typeof(DropZone));

    public static readonly DependencyProperty DropTextProperty =
        DependencyProperty.Register(nameof(DropText), typeof(string), typeof(DropZone),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AcceptedFormatsTextProperty =
        DependencyProperty.Register(nameof(AcceptedFormatsText), typeof(string), typeof(DropZone),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AcceptedExtensionsProperty =
        DependencyProperty.Register(nameof(AcceptedExtensions), typeof(string), typeof(DropZone),
            new PropertyMetadata("*.*"));

    public static readonly DependencyProperty SelectFilesTextProperty =
        DependencyProperty.Register(nameof(SelectFilesText), typeof(string), typeof(DropZone),
            new PropertyMetadata("Select files"));

    public static readonly DependencyProperty DropIconSymbolProperty =
        DependencyProperty.Register(nameof(DropIconSymbol), typeof(SymbolRegular), typeof(DropZone),
            new PropertyMetadata(SymbolRegular.CloudArrowUp24));

    public static readonly DependencyProperty DropIconFontSizeProperty =
        DependencyProperty.Register(nameof(DropIconFontSize), typeof(double), typeof(DropZone),
            new PropertyMetadata(48.0));

    public static readonly DependencyProperty DropPaddingProperty =
        DependencyProperty.Register(nameof(DropPadding), typeof(Thickness), typeof(DropZone),
            new PropertyMetadata(new Thickness(48, 40, 48, 40)));

    public static readonly DependencyProperty DropMinWidthProperty =
        DependencyProperty.Register(nameof(DropMinWidth), typeof(double), typeof(DropZone),
            new PropertyMetadata(280.0));

    public static readonly DependencyProperty DropMinHeightProperty =
        DependencyProperty.Register(nameof(DropMinHeight), typeof(double), typeof(DropZone),
            new PropertyMetadata(180.0));

    public delegate void FilesDroppedEventHandler(object sender, FilesDroppedEventArgs e);

    public event FilesDroppedEventHandler FilesDropped
    {
        add => AddHandler(FilesDroppedEvent, value);
        remove => RemoveHandler(FilesDroppedEvent, value);
    }

    public event FilesDroppedEventHandler FilesSelected
    {
        add => AddHandler(FilesSelectedEvent, value);
        remove => RemoveHandler(FilesSelectedEvent, value);
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

    public string AcceptedExtensions
    {
        get => (string)GetValue(AcceptedExtensionsProperty);
        set => SetValue(AcceptedExtensionsProperty, value);
    }

    public string SelectFilesText { get => (string)GetValue(SelectFilesTextProperty); set => SetValue(SelectFilesTextProperty, value); }
    public SymbolRegular DropIconSymbol { get => (SymbolRegular)GetValue(DropIconSymbolProperty); set => SetValue(DropIconSymbolProperty, value); }
    public double DropIconFontSize { get => (double)GetValue(DropIconFontSizeProperty); set => SetValue(DropIconFontSizeProperty, value); }
    public Thickness DropPadding { get => (Thickness)GetValue(DropPaddingProperty); set => SetValue(DropPaddingProperty, value); }
    public double DropMinWidth { get => (double)GetValue(DropMinWidthProperty); set => SetValue(DropMinWidthProperty, value); }
    public double DropMinHeight { get => (double)GetValue(DropMinHeightProperty); set => SetValue(DropMinHeightProperty, value); }

    public DropZone()
    {
        InitializeComponent();
    }

    private void DropZone_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            DropAreaBorder.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
            DropAreaBorder.SetResourceReference(Border.BackgroundProperty, "AccentLightBrush");
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        DropAreaBorder.ClearValue(Border.BorderBrushProperty);
        DropAreaBorder.ClearValue(Border.BackgroundProperty);
        e.Handled = true;
    }

    private void DropZone_Drop(object sender, DragEventArgs e)
    {
        DropAreaBorder.ClearValue(Border.BorderBrushProperty);
        DropAreaBorder.ClearValue(Border.BackgroundProperty);

        if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
            e.Data.GetData(DataFormats.FileDrop) is string[] files)
        {
            RaiseEvent(new FilesDroppedEventArgs(FilesDroppedEvent, this, files));
        }
        e.Handled = true;
    }

    private void SelectFilesButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = $"Supported files ({AcceptedExtensions})|{AcceptedExtensions}|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            RaiseEvent(new FilesDroppedEventArgs(FilesSelectedEvent, this, dialog.FileNames));
        }
    }
}

public class FilesDroppedEventArgs : RoutedEventArgs
{
    public string[] FilePaths { get; }

    public FilesDroppedEventArgs(RoutedEvent routedEvent, object source, string[] filePaths)
        : base(routedEvent, source)
    {
        FilePaths = filePaths;
    }
}
