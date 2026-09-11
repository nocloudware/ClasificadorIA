using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using NoCloudware.UI.Core.ViewModels;

namespace NoCloudware.UI.Core.Controls;

public partial class FileListBox : UserControl
{
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<BaseFileItem>),
            typeof(FileListBox),
            new PropertyMetadata(new ObservableCollection<BaseFileItem>(), OnItemsChanged));

    public static readonly DependencyProperty RemoveMenuItemTextProperty =
        DependencyProperty.Register(nameof(RemoveMenuItemText), typeof(string), typeof(FileListBox),
            new PropertyMetadata("Remove", OnMenuItemTextChanged));

    public static readonly DependencyProperty ClearAllMenuItemTextProperty =
        DependencyProperty.Register(nameof(ClearAllMenuItemText), typeof(string), typeof(FileListBox),
            new PropertyMetadata("Clear All", OnMenuItemTextChanged));

    public static readonly RoutedEvent FilesDroppedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(FilesDropped),
            RoutingStrategy.Bubble,
            typeof(DropZone.FilesDroppedEventHandler),
            typeof(FileListBox));

    private const double StatusColumnWidth = 100;
    private const double RemoveColumnWidth = 44;

    public ObservableCollection<BaseFileItem> Items
    {
        get => (ObservableCollection<BaseFileItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public string RemoveMenuItemText { get => (string)GetValue(RemoveMenuItemTextProperty); set => SetValue(RemoveMenuItemTextProperty, value); }
    public string ClearAllMenuItemText { get => (string)GetValue(ClearAllMenuItemTextProperty); set => SetValue(ClearAllMenuItemTextProperty, value); }

    public event DropZone.FilesDroppedEventHandler FilesDropped
    {
        add => AddHandler(FilesDroppedEvent, value);
        remove => RemoveHandler(FilesDroppedEvent, value);
    }

    private readonly TextBlock[] _headers = new TextBlock[4];
    private readonly GridViewColumn[] _metaColumns = new GridViewColumn[4];
    private CollectionViewSource? _cvs;

    public FileListBox()
    {
        InitializeComponent();
        _headers[0] = H0;
        _headers[1] = H1;
        _headers[2] = H2;
        _headers[3] = H3;
        _metaColumns[0] = MetaCol0;
        _metaColumns[1] = MetaCol1;
        _metaColumns[2] = MetaCol2;
        _metaColumns[3] = MetaCol3;
        BindGrouping();
        Loaded += (_, _) => ComputeColumns();
        SizeChanged += (_, _) => ComputeColumns();
    }

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FileListBox f) f.BindGrouping();
    }

    private void BindGrouping()
    {
        _cvs = new CollectionViewSource { Source = Items };
        _cvs.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BaseFileItem.Category)));
        FileListBoxControl.ItemsSource = _cvs.View;
    }

    public void RefreshGrouping() => _cvs?.View?.Refresh();

    public void SetMetadataHeaders(IReadOnlyList<string> headers)
    {
        for (int i = 0; i < _headers.Length; i++)
            _headers[i].Text = headers.Count > i ? headers[i] : "";
        ComputeColumns();
    }

    private void ComputeColumns()
    {
        double usable = Math.Max(0, ActualWidth - StatusColumnWidth - RemoveColumnWidth);
        double metaWidth = _headers[0].Text.Length > 0 ? usable / 12.0 : 0;
        NameCol.Width = Math.Max(0, usable - 4 * metaWidth);
        foreach (var column in _metaColumns)
            column.Width = metaWidth;
        StatusCol.Width = StatusColumnWidth;
        RemoveCol.Width = RemoveColumnWidth;
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) is false) return;

        var filePaths = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (filePaths is null) return;

        RaiseEvent(new FilesDroppedEventArgs(FilesDroppedEvent, this, filePaths));
    }

    private void OnRemoveClicked(object sender, RoutedEventArgs e)
    {
        if (FileListBoxControl.SelectedItem is BaseFileItem item)
        {
            Items.Remove(item);
        }
    }

    private void OnRowRemoveClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button b && b.DataContext is BaseFileItem item)
        {
            Items.Remove(item);
        }
    }

    private void OnListKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;
        foreach (var item in FileListBoxControl.SelectedItems.Cast<BaseFileItem>().ToList())
            Items.Remove(item);
    }

    private void OnClearAllClicked(object sender, RoutedEventArgs e)
    {
        Items.Clear();
    }

    private static void OnMenuItemTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FileListBox f) return;
        if (e.Property == RemoveMenuItemTextProperty && f.RemoveItem is not null)
            f.RemoveItem.Header = (string?)e.NewValue;
        else if (e.Property == ClearAllMenuItemTextProperty && f.ClearAllItem is not null)
            f.ClearAllItem.Header = (string?)e.NewValue;
    }

    private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        RemoveItem.IsEnabled = FileListBoxControl.SelectedItem is not null;
        ClearAllItem.IsEnabled = Items.Count > 0;
    }
}
