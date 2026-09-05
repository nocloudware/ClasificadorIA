using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NoCloudware.UI.Core.ViewModels;

public partial class BaseMainViewModel : ObservableObject
{
    private readonly IFileProcessor? _fileProcessor;

    // ── Observable collections ────────────────────────────────────────

    public ObservableCollection<BaseFileItem> Files { get; } = new();

    // ── Bindable properties ───────────────────────────────────────────

    [ObservableProperty]
    private string _appTitle = string.Empty;

    [ObservableProperty]
    private string _appTagline = string.Empty;

    [ObservableProperty]
    private string _actionButtonText = "Action";

    [ObservableProperty]
    private string _aboutButtonText = "About";

    [ObservableProperty]
    private string _donateButtonText = "Donate";

    [ObservableProperty]
    private string _exitButtonText = "Exit";

    [ObservableProperty]
    private string _dropText = "";

    [ObservableProperty]
    private string _acceptedFormatsText = "";

    [ObservableProperty]
    private string _outputFolder = "";

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _processedCount;

    [ObservableProperty]
    private int _pendingCount;

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private bool _isProcessing;

    // ── Constructor ───────────────────────────────────────────────────

    public BaseMainViewModel(IFileProcessor? fileProcessor = null)
    {
        _fileProcessor = fileProcessor;
        if (_fileProcessor != null)
        {
            AcceptedFormatsText = _fileProcessor.GetAcceptedFormatsText();
        }
    }

    // ── File operations ───────────────────────────────────────────────

    public void AddFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (Files.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (_fileProcessor != null && !_fileProcessor.IsValidFile(path))
                continue;

            Files.Add(new BaseFileItem
            {
                FilePath = path,
                FileName = Path.GetFileName(path),
                FileSize = new FileInfo(path).Length
            });
        }
        UpdateCounters();
    }

    public void RemoveFile(BaseFileItem item)
    {
        Files.Remove(item);
        UpdateCounters();
    }

    public void ClearFiles()
    {
        Files.Clear();
        UpdateCounters();
    }

    // ── Counters ──────────────────────────────────────────────────────

    public void UpdateCounters()
    {
        TotalCount = Files.Count;
        ProcessedCount = Files.Count(f => f.Status == FileStatus.Processed);
        PendingCount = Files.Count(f => f.Status is FileStatus.Queued or FileStatus.Processing);
        ErrorCount = Files.Count(f => f.Status == FileStatus.Error);
    }

    // ── Commands ──────────────────────────────────────────────────────

    [RelayCommand]
    protected virtual void OnAbout() { }

    [RelayCommand]
    protected virtual void OnDonate() { }

    [RelayCommand]
    protected virtual async Task OnAction()
    {
        if (IsProcessing || _fileProcessor == null) return;
        IsProcessing = true;

        try
        {
            var items = Files.Where(f => f.Status is FileStatus.Queued or FileStatus.PendingManual).ToList();
            foreach (var item in items)
            {
                item.Status = FileStatus.Processing;
                UpdateCounters();

                var result = await _fileProcessor.ProcessAsync(item.FilePath, OutputFolder);

                item.Status = result.Success ? FileStatus.Processed : FileStatus.Error;
                item.ResultMessage = result.Message ?? string.Empty;
                item.OutputPath = result.OutputPath ?? string.Empty;
                item.CanOpen = result.Success;
                item.CanRetry = !result.Success;
                UpdateCounters();
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    protected virtual void OnExit()
    {
        System.Windows.Application.Current?.Shutdown();
    }

    // ── File selection ────────────────────────────────────────────────

    public void OnFilesDropped(string[] paths)
    {
        AddFiles(paths);
    }

    public void OnFilesSelected(string[] paths)
    {
        AddFiles(paths);
    }
}
