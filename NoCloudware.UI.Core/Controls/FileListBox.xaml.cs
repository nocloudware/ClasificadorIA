using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using NoCloudware.UI.Core.ViewModels;

namespace NoCloudware.UI.Core.Controls;

public partial class FileListBox : UserControl
{
    private static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;
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

    public static readonly DependencyProperty IsBusyProperty =
        DependencyProperty.Register(nameof(IsBusy), typeof(bool), typeof(FileListBox),
            new PropertyMetadata(false, OnIsBusyChanged));

    public static readonly RoutedEvent FilesDroppedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(FilesDropped),
            RoutingStrategy.Bubble,
            typeof(DropZone.FilesDroppedEventHandler),
            typeof(FileListBox));

    private const double ScrollbarReserve = 18;
    private const double RemoveColumnWidth = 44;

    public ObservableCollection<BaseFileItem> Items
    {
        get => (ObservableCollection<BaseFileItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public string RemoveMenuItemText { get => (string)GetValue(RemoveMenuItemTextProperty); set => SetValue(RemoveMenuItemTextProperty, value); }
    public string ClearAllMenuItemText { get => (string)GetValue(ClearAllMenuItemTextProperty); set => SetValue(ClearAllMenuItemTextProperty, value); }
    public bool IsBusy { get => (bool)GetValue(IsBusyProperty); set => SetValue(IsBusyProperty, value); }

    public event DropZone.FilesDroppedEventHandler FilesDropped
    {
        add => AddHandler(FilesDroppedEvent, value);
        remove => RemoveHandler(FilesDroppedEvent, value);
    }

    private readonly TextBlock[] _headers = new TextBlock[4];
    private readonly GridViewColumn[] _metaColumns = new GridViewColumn[4];
    private readonly ObservableCollection<object> _rows = new();
    private readonly HashSet<string> _collapsed = new(StringComparer.OrdinalIgnoreCase);
    private bool _showCategories;
    private Action? _itemsHook;
    private bool _rebuildPending;

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
        FileListBoxControl.ItemsSource = _rows;
        UpdateItemsSubscription();
        Loaded += (_, _) => ComputeColumns();
        SizeChanged += (_, _) => ComputeColumns();
    }

    public IReadOnlyList<object> Rows => _rows;

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FileListBox f)
        {
            f.UpdateItemsSubscription();
            f.ScheduleRebuild();
        }
    }

    private void UpdateItemsSubscription()
    {
        _itemsHook?.Invoke();
        _itemsHook = null;
        if (Items is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += OnItemsCollectionChanged;
            _itemsHook = () => incc.CollectionChanged -= OnItemsCollectionChanged;
        }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => ScheduleRebuild();

    private void ScheduleRebuild()
    {
        if (_rebuildPending) return;
        _rebuildPending = true;
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
        {
            _rebuildPending = false;
            RebuildRows();
        }));
    }

    public void SetShowCategories(bool value)
    {
        _showCategories = value;
        _collapsed.Clear();
        if (value)
            foreach (var category in Items.Select(f => f.Category).Distinct(StringComparer.OrdinalIgnoreCase))
                _collapsed.Add(category);
        RebuildRows();
    }

    public bool ShowCategories => _showCategories;

    public void ToggleToggle(string key)
    {
        if (!_collapsed.Add(key)) _collapsed.Remove(key);
        RebuildRows();
    }

    private void RebuildRows()
    {
        if (_rebuildPending) _rebuildPending = false;
        _rows.Clear();
        if (Items.Count == 0) return;
        foreach (var row in _showCategories ? BuildCategoryRows() : BuildFolderRows())
            _rows.Add(row);
    }

    private static string RootOf(BaseFileItem f) =>
        !string.IsNullOrWhiteSpace(f.SourceFolder)
            ? f.SourceFolder.TrimEnd('\\')
            : (Path.GetDirectoryName(f.FilePath) ?? f.FilePath);

    private static string FolderDisplayName(string key) =>
        string.IsNullOrEmpty(Path.GetFileName(key)) ? key : Path.GetFileName(key);

    private IEnumerable<object> BuildFolderRows()
    {
        var roots = Items
            .GroupBy(RootOf, PathComparer)
            .OrderBy(g => g.Key, PathComparer);
        foreach (var grp in roots)
        {
            var rootKey = grp.Key;
            var rootRow = new TreeFolderNode
            {
                Key = rootKey,
                DisplayName = FolderDisplayName(rootKey),
                IsExpanded = !_collapsed.Contains(rootKey),
                Depth = 0
            };
            BuildNested(rootRow, rootKey, grp.ToList());
            if (rootRow.Folders.Count == 0 && rootRow.Files.Count == 0) continue;
            rootRow.Count = CountFiles(rootRow);
            foreach (var row in FlattenFolder(rootRow))
                yield return row;
        }
    }

    private void BuildNested(TreeFolderNode node, string rootKey, List<BaseFileItem> files)
    {
        foreach (var f in files)
        {
            var dir = Path.GetDirectoryName(f.FilePath) ?? rootKey;
            var rel = Path.GetRelativePath(rootKey, dir);
            var current = node;
            var ownerKey = rootKey;
            if (rel != "." && !string.IsNullOrEmpty(rel))
            {
                foreach (var seg in rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                {
                    if (string.IsNullOrEmpty(seg) || seg == ".") continue;
                    var full = Path.Combine(ownerKey, seg);
                    var child = current.Folders.FirstOrDefault(x => PathComparer.Equals(x.Key, full));
                    if (child == null)
                    {
                        child = new TreeFolderNode
                        {
                            Key = full,
                            DisplayName = seg,
                            IsExpanded = !_collapsed.Contains(full),
                            Depth = current.Depth + 1
                        };
                        current.Folders.Add(child);
                    }
                    current = child;
                    ownerKey = full;
                }
            }
            current.Files.Add(f);
        }
    }

    private static int CountFiles(TreeFolderNode n) =>
        n.Files.Count + n.Folders.Sum(CountFiles);

    private IEnumerable<object> FlattenFolder(TreeFolderNode n)
    {
        yield return n;
        if (!n.IsExpanded) yield break;
        foreach (var sub in n.Folders.OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase))
            foreach (var row in FlattenFolder(sub))
                yield return row;
        foreach (var file in n.Files.OrderBy(x => x.FileName, StringComparer.OrdinalIgnoreCase))
        {
            file.TreeDepth = n.Depth + 1;
            yield return file;
        }
    }

    private IEnumerable<object> BuildCategoryRows()
    {
        var groups = Items
            .GroupBy(f => f.Category, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);
        foreach (var grp in groups)
        {
            var row = new TreeFolderNode
            {
                Key = grp.Key,
                DisplayName = grp.Key,
                IsExpanded = !_collapsed.Contains(grp.Key),
                IsCategory = true,
                Depth = 0,
                Count = grp.Count()
            };
            row.Files.AddRange(grp.OrderBy(f => f.FileName, StringComparer.OrdinalIgnoreCase));
            yield return row;
            if (row.IsExpanded)
            {
                foreach (var f in row.Files)
                {
                    f.TreeDepth = 1;
                    yield return f;
                }
            }
        }
    }

    private void OnToggleFolderClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TreeFolderNode node)
            ToggleToggle(node.Key);
    }

    private void RemoveRow(object row)
    {
        if (row is BaseFileItem item) { Items.Remove(item); return; }
        if (row is TreeFolderNode node)
        {
            var victims = node.Files.Concat(node.Folders.SelectMany(FlattenFiles)).ToList();
            foreach (var v in victims)
                Items.Remove(v);
        }
    }

    private static IEnumerable<BaseFileItem> FlattenFiles(TreeFolderNode n) =>
        n.Files.Concat(n.Folders.SelectMany(FlattenFiles));

    public void SetMetadataHeaders(IReadOnlyList<string> headers)
    {
        for (int i = 0; i < _headers.Length; i++)
            _headers[i].Text = headers.Count > i ? headers[i] : "";
        ComputeColumns();
    }

    private void ComputeColumns()
    {
        double usable = Math.Max(0, ActualWidth - RemoveColumnWidth - ScrollbarReserve);
        double metaWidth = _headers[0].Text.Length > 0 ? usable / 8.0 : 0;
        NameCol.Width = Math.Max(0, usable - 4 * metaWidth);
        foreach (var column in _metaColumns)
            column.Width = metaWidth;
        RemoveCol.Width = RemoveColumnWidth;
    }

    private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FileListBox f && f.BusyOverlay is not null)
        {
            bool busy = (bool)e.NewValue;
            f.BusyOverlay.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
            f.BusyOverlay.IsHitTestVisible = busy;
        }
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
        foreach (var selected in FileListBoxControl.SelectedItems.Cast<object>().ToList())
            RemoveRow(selected);
    }

    private void OnRowRemoveClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button b)
            RemoveRow(b.DataContext!);
    }

    private void OnListKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;
        foreach (var selected in FileListBoxControl.SelectedItems.Cast<object>().ToList())
            RemoveRow(selected);
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

public sealed class FileRowSelector : DataTemplateSelector
{
    public DataTemplate? FileTemplate { get; set; }
    public DataTemplate? FolderTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        => item is TreeFolderNode ? FolderTemplate : FileTemplate;
}
