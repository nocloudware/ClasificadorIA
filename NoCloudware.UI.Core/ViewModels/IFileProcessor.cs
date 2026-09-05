namespace NoCloudware.UI.Core.ViewModels;

public interface IFileProcessor
{
    Task<FileProcessingResult> ProcessAsync(string filePath, string outputFolder);
    bool IsValidFile(string filePath);
    string[] GetAcceptedExtensions();
    string GetAcceptedFormatsText();
}
