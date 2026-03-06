using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartService.Application.Abstractions.AI;
using System.Diagnostics;
using System.Text;

namespace SmartService.Infrastructure.AI.Tesseract;

/// <summary>
/// OCR service implementation using local Tesseract binary (tesseract.exe).
/// Saves image to a temp file, runs Tesseract as a child process, reads stdout result.
/// </summary>
public sealed class TesseractOcrService : IOcrService
{
    private readonly TesseractSettings _settings;
    private readonly ILogger<TesseractOcrService> _logger;

    public TesseractOcrService(
        IOptions<TesseractSettings> settings,
        ILogger<TesseractOcrService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(
        Stream imageStream,
        string fileName,
        CancellationToken ct = default)
    {
        // Determine temp file extension from original filename
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";

        var tempImagePath = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid()}{ext}");
        var tempOutputBase = Path.Combine(Path.GetTempPath(), $"ocr_out_{Guid.NewGuid()}");
        var tempOutputTxt = tempOutputBase + ".txt";

        try
        {
            // Write image to temp file
            await using (var fs = File.Create(tempImagePath))
            {
                await imageStream.CopyToAsync(fs, ct);
            }

            // Build Tesseract arguments: <input> <output_base> -l <lang> --psm 3
            var arguments = $"\"{tempImagePath}\" \"{tempOutputBase}\" -l {_settings.Language} --psm 3";

            var psi = new ProcessStartInfo
            {
                FileName = _settings.ExePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_settings.TimeoutSeconds));

            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                _logger.LogWarning("Tesseract OCR timed out after {Timeout}s for file {FileName}",
                    _settings.TimeoutSeconds, fileName);
                return string.Empty;
            }

            if (process.ExitCode != 0)
            {
                var errOutput = await process.StandardError.ReadToEndAsync(ct);
                _logger.LogWarning("Tesseract exited with code {Code}. Stderr: {Err}", process.ExitCode, errOutput);
                return string.Empty;
            }

            // Read the output .txt file Tesseract writes
            if (!File.Exists(tempOutputTxt))
            {
                _logger.LogWarning("Tesseract output file not found: {Path}", tempOutputTxt);
                return string.Empty;
            }

            var text = await File.ReadAllTextAsync(tempOutputTxt, Encoding.UTF8, ct);
            return text.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR extraction failed for file {FileName}", fileName);
            return string.Empty;
        }
        finally
        {
            // Clean up temp files
            TryDelete(tempImagePath);
            TryDelete(tempOutputTxt);
        }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* best effort */ }
    }
}
