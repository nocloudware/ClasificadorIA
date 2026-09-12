using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NoCloudware.UI.Core.ViewModels;

public partial class BaseFileItem : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString("N");

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private long _fileSize;

    [ObservableProperty]
    private FileStatus _status = FileStatus.Queued;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private string _resultMessage = string.Empty;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    private bool _canRetry;

    [ObservableProperty]
    private bool _canOpen;
[ObservableProperty]
    private string[] _metadataCells = Array.Empty<string>();

    [ObservableProperty]
    private Dictionary<string, string?> _allMetadata = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    private string _category = "Archivos";

    [ObservableProperty]
    private string _sourceFolder = string.Empty;

    [ObservableProperty]
    private int _treeDepth;

    public Thickness TreeIndent => new(Math.Max(0, TreeDepth) * 14 + 8, 3, 0, 3);
}
