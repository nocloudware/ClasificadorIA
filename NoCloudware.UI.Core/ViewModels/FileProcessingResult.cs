namespace NoCloudware.UI.Core.ViewModels;

public class FileProcessingResult
{
    public bool Success { get; set; }
    public string? OutputPath { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
}
