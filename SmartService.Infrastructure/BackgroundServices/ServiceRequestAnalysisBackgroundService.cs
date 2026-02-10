using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartService.Application.Abstractions.AI;
using SmartService.Application.Abstractions.Notifications;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;
using SmartService.Infrastructure.Persistence;

namespace SmartService.Infrastructure.BackgroundServices;

public class ServiceRequestAnalysisBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceRequestAnalysisBackgroundService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    public ServiceRequestAnalysisBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ServiceRequestAnalysisBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingAnalysesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing service request analyses");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingAnalysesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var aiAnalyzer = scope.ServiceProvider.GetRequiredService<IAiAnalyzer>();
        var notificationService = scope.ServiceProvider.GetRequiredService<IServiceRequestNotificationService>();

        var pendingRequests = await context.ServiceRequests
            .Where(x => x.Status == ServiceStatus.AwaitingAnalysis)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var request in pendingRequests)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Description))
                    continue;

                var aiResult = await aiAnalyzer.AnalyzeAsync(request.Description, cancellationToken);

                var analysis = ServiceAnalysis.Create(
                    request.Id,
                    aiResult.ComplexityLevel,
                    aiResult.UrgencyLevel,
                    aiResult.Context.SafetyAdvice,
                    aiResult.Context.Summary);

                context.ServiceAnalyses.Add(analysis);
                
                request.MarkAsAnalyzed(aiResult.UrgencyLevel);

                await context.SaveChangesAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(aiResult.Context.SafetyAdvice))
                {
                    await notificationService.SendSafetyAdviceAsync(
                        request.Id,
                        aiResult.Context.SafetyAdvice,
                        aiResult.UrgencyLevel,
                        cancellationToken);
                }

                _logger.LogInformation(
                    "Analyzed service request {RequestId}: Complexity={Complexity}, Urgency={Urgency}",
                    request.Id, aiResult.ComplexityLevel, aiResult.UrgencyLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing service request {RequestId}", request.Id);
            }
        }
    }
}
