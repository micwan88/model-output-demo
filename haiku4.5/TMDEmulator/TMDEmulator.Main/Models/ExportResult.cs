namespace TMDEmulator.Main.Models;

public class ExportResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? Fingerprint { get; set; }
    public string? ErrorMessage { get; set; }

    public ExportResult(bool success, string? filePath = null, string? fingerprint = null, string? errorMessage = null)
    {
        Success = success;
        FilePath = filePath;
        Fingerprint = fingerprint;
        ErrorMessage = errorMessage;
    }
}
