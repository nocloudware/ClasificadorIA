namespace NoCloudware.UI.Core.ViewModels;

public interface IDropZoneHandler
{
    void OnFilesDropped(string[] filePaths);
    bool CanAcceptFile(string filePath);
}
