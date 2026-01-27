using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartService.Application.Abstractions.AI;
using SmartService.Application.Abstractions.Auth;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Infrastructure.AI.Ollama;
using SmartService.Infrastructure.Auth;
using SmartService.Infrastructure.KnowledgeBase.Complexity;
using SmartService.Infrastructure.Persistence;

namespace SmartService.Infrastructure;

/// <summary>
/// Dependency Injection configuration for Infrastructure layer.
/// Responsible for registering:
/// - Database context (AppDbContext)
/// - External service implementations (AI, Knowledge Base)
/// - Repository or Query implementations if applicable
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register the database context factory
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        // Register AppDbContext instance and its abstraction
        // This allows handlers to depend on IAppDbContext interface
        services.AddScoped<AppDbContext>();
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Register Knowledge Base and AI services
        services.AddSingleton<ComplexityRuleProvider>();
        services.AddSingleton<SimpleComplexityMatcher>();
        services.AddScoped<IAiAnalyzer, OllamaAiAnalyzer>();

        // Register Authentication services
        // Configure TokenConfiguration from appsettings.json
        services.Configure<TokenConfiguration>(configuration.GetSection("JwtSettings"));
        
        // Register auth services
        services.AddScoped<JwtTokenService>();
        services.AddScoped<AesEncryptionService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthService, AuthService>();

        // Register notification service (will be implemented in WebAPI layer with SignalR)
        // Note: IServiceRequestNotificationService implementation is registered in Program.cs
        // because it requires SignalR HubContext which is only available in WebAPI layer

        return services;
    }
}

