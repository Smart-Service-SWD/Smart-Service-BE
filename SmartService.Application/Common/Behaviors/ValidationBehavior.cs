using FluentValidation;
using MediatR;
using SmartService.Domain.Exceptions;

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
        if (failures.Count != 0)
        {
            var errorMessage = failures.First().ErrorMessage;
            throw new BusinessRuleException(errorMessage);
        }

        return await next();
    }
}
