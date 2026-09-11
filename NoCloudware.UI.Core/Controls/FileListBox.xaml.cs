using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            new PropertyMetadata(new ObservableCollection<BaseFileItem>()));

    public static readonly DependencyProperty RemoveMenuItemTextProperty =
        DependencyProperty.Register(nameof(RemoveMenuItemText), typeof(string), typeof(FileListBox),
            new PropertyMetadata("Remove"));

    public static readonly DependencyProperty ClearAllMenuItemTextProperty =
        DependencyProperty.Register(nameof(ClearAllMenuItemText), typeof(string), typeof(FileListBox),
            new PropertyMetadata("Clear All"));

    public static readonly DependencyProperty ItemMarginProperty =
        DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(FileListBox),
            new PropertyMetadata(new Thickness(0, 3, 0, 3)));

    public static readonly RoutedEvent FilesDroppedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(FilesDropped),
            RoutingStrategy.Bubble,
            typeof(DropZone.FilesDroppedEventHandler),
            typeof(FileListBox));

    public ObservableCollection<BaseFileItem> Items
    {
        get => (ObservableCollection<BaseFileItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public string RemoveMenuItemText { get => (string)GetValue(RemoveMenuItemTextProperty); set => SetValue(RemoveMenuItemTextProperty, value); }
    public string ClearAllMenuItemText { get => (string)GetValue(ClearAllMenuItemTextProperty); set => SetValue(ClearAllMenuItemTextProperty, value); }
    public Thickness ItemMargin { get => (Thickness)GetValue(ItemMarginProperty); set => SetValue(ItemMarginProperty, value); }

    public event DropZone.FilesDroppedEventHandler FilesDropped
    {
        add => AddHandler(FilesDroppedEvent, value);
        remove => RemoveHandler(FilesDroppedEvent, value);
    }

    public FileListBox()
    {
        InitializeComponent();
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

    private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (FileListBoxControl.ContextMenu is not { } menu) return;

        foreach (var item in menu.Items.OfType<MenuItem>())
        {
            item.IsEnabled = item.Header.ToString() switch
            {
                string s when s == RemoveMenuItemText => FileListBoxControl.SelectedItem is not null,
                string s when s == ClearAllMenuItemText => Items.Count > 0,
                _ => true
            };
        }
    }
}
