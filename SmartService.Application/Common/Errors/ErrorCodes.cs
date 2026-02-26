namespace SmartService.Application.Common.Errors;

/// <summary>
/// Standardized error codes for the application.
/// Format: MODULE_HTTPCODE_DESCRIPTION
/// </summary>
public static class ErrorCodes
{
    // Authentication & Security
    public const string AUTH_401_INVALID_CREDENTIALS = "AUTH_401_INVALID_CREDENTIALS";
    public const string AUTH_401_UNAUTHORIZED = "AUTH_401_UNAUTHORIZED";
    public const string AUTH_403_FORBIDDEN = "AUTH_403_FORBIDDEN";
    public const string AUTH_400_INVALID_TOKEN = "AUTH_400_INVALID_TOKEN";
    public const string AUTH_409_EMAIL_EXISTS = "AUTH_409_EMAIL_EXISTS";
    public const string AUTH_403_ACCESS_DENIED = "AUTH_403_ACCESS_DENIED";

    // Validation
    public const string REQUEST_400_VALIDATION_FAILED = "REQUEST_400_VALIDATION_FAILED";

    // General Business Rules
    public const string BUSINESS_400_INVALID_OPERATION = "BUSINESS_400_INVALID_OPERATION";
    public const string BUSINESS_404_NOT_FOUND = "BUSINESS_404_NOT_FOUND";
    public const string BUSINESS_409_CONFLICT = "BUSINESS_409_CONFLICT";

    // Resource Specific (Can be expanded as needed)
    public const string USER_404_NOT_FOUND = "USER_404_NOT_FOUND";
    public const string SERVICE_404_NOT_FOUND = "SERVICE_404_NOT_FOUND";
    public const string REQUEST_404_NOT_FOUND = "REQUEST_404_NOT_FOUND";

    // System Errors
    public const string SYSTEM_500_INTERNAL_SERVER_ERROR = "SYSTEM_500_INTERNAL_SERVER_ERROR";
}
