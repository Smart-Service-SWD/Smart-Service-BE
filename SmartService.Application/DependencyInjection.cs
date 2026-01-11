using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace SmartService.Application;

/// <summary>
/// Dependency Injection configuration for Application layer.
/// Responsible for registering application services such as:
/// - MediatR handlers (Commands, Queries)
/// - Application-level behaviors and pipelines
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}
