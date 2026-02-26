namespace SmartService.Application.Common.Errors;

/// <summary>
/// Standardized error response for the API.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Indicates success status (always false for error responses).
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// Machine-readable error code for frontend logic.
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable message for user display.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional details (e.g., validation errors).
    /// </summary>
    public object? Details { get; set; }

    /// <summary>
    /// Trace ID for debugging in production.
    /// </summary>
    public string TraceId { get; set; } = string.Empty;
}
