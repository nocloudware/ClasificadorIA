using System.Windows;

namespace NoCloudware.UI.Core.ViewModels;

public sealed class TreeFolderNode
{
    public required string Key { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public int Depth { get; init; }
    public bool IsExpanded { get; set; }
    public bool IsCategory { get; init; }

    public List<TreeFolderNode> Folders { get; } = new();
    public List<BaseFileItem> Files { get; } = new();
    public List<BaseFileItem> AllFiles { get; } = new();

    public string ExpanderGlyph => IsExpanded ? "▼" : "▶";
    public string CountText => Files.Count > 0 || Folders.Count > 0 ? $" ({Count})" : "";
    public int Count { get; set; }

    public string[] MetadataCells => Array.Empty<string>();
    public Thickness TreeIndent => new(8 + Math.Max(0, Depth) * 14, 3, 0, 3);
}