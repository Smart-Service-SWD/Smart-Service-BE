using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartService.Application.Common.Behaviors;
using System.Reflection;

namespace SmartService.Application;

/// <summary>
/// Dependency Injection configuration for Application layer.
/// Responsible for registering:
/// - MediatR handlers (Commands, Queries)
/// - FluentValidation validators
/// - Pipeline behaviors (Validation, Logging, etc.)
/// 
/// This configuration ensures that all Commands and Queries are validated
/// before reaching their handlers through the ValidationBehavior pipeline.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all validators from the Application assembly
        // FluentValidation will automatically discover and register IValidator implementations
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddValidatorsFromAssembly(assembly);

        // Register MediatR and all handlers from the Application assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            
            // Register the ValidationBehavior pipeline
            // This ensures validation runs before any handler is executed
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
