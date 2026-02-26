using System.Net;
using System.Text.Json;
using FluentValidation;
using AppErrorCodes = SmartService.Application.Common.Errors.ErrorCodes;
using SmartService.Application.Common.Errors;
using SmartService.Domain.Exceptions;

namespace SmartService.API.Middleware;

/// <summary>
/// Global exception handling middleware to catch and standardize all errors.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, errorCode, errorType, message, details) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                AppErrorCodes.REQUEST_400_VALIDATION_FAILED,
                "Validation",
                "Validation failed.",
                (object)validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            ),

            AuthException.InvalidCredentialsException => (
                HttpStatusCode.Unauthorized,
                AppErrorCodes.AUTH_401_INVALID_CREDENTIALS,
                "Security",
                "Invalid credentials.",
                null
            ),

            AuthException.EmailAlreadyRegisteredException => (
                HttpStatusCode.Conflict,
                AppErrorCodes.AUTH_409_EMAIL_EXISTS,
                "Business",
                exception.Message,
                null
            ),

            AuthException.UserNotFoundException => (
                HttpStatusCode.NotFound,
                AppErrorCodes.USER_404_NOT_FOUND,
                "Business",
                exception.Message,
                null
            ),

            AuthException => (
                HttpStatusCode.Unauthorized,
                AppErrorCodes.AUTH_401_UNAUTHORIZED,
                "Security",
                exception.Message,
                null
            ),

            BusinessRuleException => (
                HttpStatusCode.BadRequest,
                AppErrorCodes.REQUEST_400_VALIDATION_FAILED,
                "Validation",
                exception.Message,
                null
            ),

            // Specific ServiceRequest exceptions
            ServiceRequestException.InvalidStateTransitionException => (
                HttpStatusCode.Conflict,
                AppErrorCodes.BUSINESS_409_CONFLICT,
                "Business",
                exception.Message,
                null
            ),

            System.Collections.Generic.KeyNotFoundException => (
                HttpStatusCode.NotFound,
                AppErrorCodes.BUSINESS_404_NOT_FOUND,
                "Business",
                exception.Message,
                null
            ),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                AppErrorCodes.REQUEST_400_VALIDATION_FAILED,
                "Validation",
                exception.Message,
                null
            ),

            DomainException => (
                HttpStatusCode.BadRequest,
                AppErrorCodes.BUSINESS_400_INVALID_OPERATION,
                "Business",
                exception.Message,
                null
            ),

            UnauthorizedAccessException => (
                HttpStatusCode.Forbidden,
                AppErrorCodes.AUTH_403_FORBIDDEN,
                "Security",
                "Access denied.",
                null
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                AppErrorCodes.SYSTEM_500_INTERNAL_SERVER_ERROR,
                "System",
                _env.IsDevelopment() ? exception.Message : "An unexpected error occurred on the server.",
                null
            )
        };

        if ((int)statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message} [TraceId: {TraceId}]", exception.Message, traceId);
        }
        else
        {
            _logger.LogWarning("Request failed: {Message} [StatusCode: {StatusCode}, TraceId: {TraceId}]", exception.Message, statusCode, traceId);
        }

        var errorResponse = new ErrorResponse
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            Details = details,
            TraceId = traceId
        };

        // Standardized Headers
        context.Response.Headers.Append("X-Error-Code", errorCode);
        context.Response.Headers.Append("X-Error-Type", errorType);
        context.Response.Headers.Append("X-Trace-Id", traceId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}
