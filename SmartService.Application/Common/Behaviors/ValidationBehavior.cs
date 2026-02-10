using FluentValidation;
using MediatR;
using SmartService.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace SmartService.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for automatic validation of MediatR requests.
/// Scans for all registered IValidator implementations for the request type
/// and executes them before passing to the handler.
/// 
/// If validation fails, throws BusinessRuleException with the first error message.
/// This ensures handlers receive only validated, business-rule-compliant data.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        // Run all validators in parallel and collect results
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Extract all failures from all validators
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
        
        // If there are validation failures, throw exception with first error message
        if (failures.Count > 0)
        {
            var firstFailure = failures.First();
            var errorMessage = firstFailure.ErrorMessage;
            var fieldName = firstFailure.PropertyName;

            // Determine the appropriate exception type based on the failure message
            if (errorMessage.Contains("required", StringComparison.OrdinalIgnoreCase))
                throw new BusinessRuleException.RequiredFieldException(fieldName);

            if (errorMessage.Contains("length", StringComparison.OrdinalIgnoreCase))
            {
                var maxLength = ExtractMaxLengthFromErrorMessage(errorMessage);
                throw new BusinessRuleException.FieldLengthExceededException(fieldName, maxLength);
            }

            if (errorMessage.Contains("range", StringComparison.OrdinalIgnoreCase))
            {
                var range = ExtractValueRangeFromErrorMessage(errorMessage);
                throw new BusinessRuleException.NumericValueOutOfRangeException(
                    fieldName, range.min, range.max);
            }

            if (errorMessage.Contains("format", StringComparison.OrdinalIgnoreCase))
            {
                var expectedFormat = ExtractExpectedFormatFromErrorMessage(errorMessage);
                throw new BusinessRuleException.InvalidFormatException(fieldName, expectedFormat);
            }

            throw new BusinessRuleException.BusinessConstraintViolationException(fieldName, errorMessage);
        }

        return await next();
    }
    private int ExtractMaxLengthFromErrorMessage(string errorMessage)
    {
        var match = Regex.Match(errorMessage, @"\d+"); // Match first numeric value
        return match.Success ? int.Parse(match.Value) : 0; // Default to 0 if no match found
    }

    private (decimal min, decimal max) ExtractValueRangeFromErrorMessage(string errorMessage)
    {
        var match = Regex.Match(errorMessage, @"between\s(\d+)\sand\s(\d+)"); // Match range
        if (match.Success)
        {
            var min = decimal.Parse(match.Groups[1].Value);
            var max = decimal.Parse(match.Groups[2].Value);
            return (min, max);
        }
        return (0, 0); // Default range
    }

    private string ExtractExpectedFormatFromErrorMessage(string errorMessage)
    {
        if (errorMessage.Contains("email", StringComparison.OrdinalIgnoreCase))
            return "example@domain.com";
        if (errorMessage.Contains("phone", StringComparison.OrdinalIgnoreCase))
            return "+1234567890";

        return "Unknown format";
    }
}
