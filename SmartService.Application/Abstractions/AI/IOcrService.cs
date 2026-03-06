namespace SmartService.Application.Abstractions.AI;

/// <summary>
/// Abstraction for OCR (Optical Character Recognition) service.
/// Extracts text from image streams using an underlying OCR engine (e.g. Tesseract).
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Extracts text from an image stream.
    /// </summary>
    /// <param name="imageStream">The image content as a stream.</param>
    /// <param name="fileName">Original file name (used to determine temp file extension).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Extracted text from the image, or empty string if extraction fails.</returns>
    Task<string> ExtractTextAsync(Stream imageStream, string fileName, CancellationToken ct = default);
}
