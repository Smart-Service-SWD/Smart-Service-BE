using HotChocolate;
using AppErrorCodes = SmartService.Application.Common.Errors.ErrorCodes;
using SmartService.Application.Common.Errors;
using SmartService.Domain.Exceptions;
using FluentValidation;

namespace SmartService.API.GraphQL;

/// <summary>
/// GraphQL error filter to standardize error codes and extensions.
/// </summary>
public class StandardErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        // If it's a GraphQL-level error without an exception, just return it
        if (error.Exception is null)
        {
            return error;
        }

        var exception = error.Exception;
        string errorCode = AppErrorCodes.SYSTEM_500_INTERNAL_SERVER_ERROR;
        string errorType = "System";

        // Map exception to standardized error code and type
        // Matching logic similar to ExceptionHandlingMiddleware
        if (exception is ValidationException)
        {
            errorCode = AppErrorCodes.REQUEST_400_VALIDATION_FAILED;
            errorType = "Validation";
        }
        else if (exception is AuthException.InvalidCredentialsException)
        {
            errorCode = AppErrorCodes.AUTH_401_INVALID_CREDENTIALS;
            errorType = "Security";
        }
        else if (exception is AuthException.EmailAlreadyRegisteredException)
        {
            errorCode = AppErrorCodes.AUTH_409_EMAIL_EXISTS;
            errorType = "Business";
        }
        else if (exception is AuthException)
        {
            errorCode = AppErrorCodes.AUTH_401_UNAUTHORIZED;
            errorType = "Security";
        }
        else if (exception is BusinessRuleException)
        {
            errorCode = AppErrorCodes.REQUEST_400_VALIDATION_FAILED;
            errorType = "Validation";
        }
        else if (exception is System.Collections.Generic.KeyNotFoundException)
        {
            errorCode = AppErrorCodes.BUSINESS_404_NOT_FOUND;
            errorType = "Business";
        }
        else if (exception is DomainException)
        {
            errorCode = AppErrorCodes.BUSINESS_400_INVALID_OPERATION;
            errorType = "Business";
        }
        else if (exception is UnauthorizedAccessException)
        {
            errorCode = AppErrorCodes.AUTH_403_FORBIDDEN;
            errorType = "Security";
        }

        return error
            .WithCode(errorCode)
            .SetExtension("errorType", errorType)
            .SetExtension("success", false);
    }
}
