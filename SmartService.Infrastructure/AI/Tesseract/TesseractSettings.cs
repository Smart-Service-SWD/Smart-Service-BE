namespace SmartService.Infrastructure.AI.Tesseract;

/// <summary>
/// Configuration settings for the Tesseract OCR engine.
/// Loaded from appsettings.json section "TesseractSettings".
/// </summary>
public class TesseractSettings
{
    /// <summary>Absolute path to tesseract.exe on the server.</summary>
    public string ExePath { get; set; } = @"D:\OCR\tesseract.exe";

    /// <summary>Language(s) to use for OCR. Use '+' to combine multiple (e.g. "vie+eng").</summary>
    public string Language { get; set; } = "vie+eng";

    /// <summary>Maximum execution time in seconds before process is killed.</summary>
    public int TimeoutSeconds { get; set; } = 30;
}
