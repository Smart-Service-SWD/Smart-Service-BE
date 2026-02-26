using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartService.Application.Abstractions.KnowledgeBase;
using SmartService.Application.Abstractions.Persistence;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartService.Infrastructure.KnowledgeBase.Sync;

/// <summary>
/// Synchronizes ServiceDefinition data from DB to KnowledgeBase JSON files.
/// 
/// Output format matches existing KnowledgeBase JSON structure:
/// - services/{group}/{subcategory}.json  → service info with cases
/// - pricing/{group}/{subcategory}_pricing.json → pricing by complexity
/// 
/// This allows AI to continue reading from JSON files while admin manages data via DB.
/// </summary>
public class KnowledgeBaseSyncService : IKnowledgeBaseSyncService
{
    private readonly IDbContextFactory<SmartService.Infrastructure.Persistence.AppDbContext> _dbFactory;
    private readonly IHostEnvironment _env;
    private readonly ILogger<KnowledgeBaseSyncService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public KnowledgeBaseSyncService(
        IDbContextFactory<SmartService.Infrastructure.Persistence.AppDbContext> dbFactory,
        IHostEnvironment env,
        ILogger<KnowledgeBaseSyncService> logger)
    {
        _dbFactory = dbFactory;
        _env = env;
        _logger = logger;
    }

    public async Task SyncAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting KnowledgeBase sync from DB...");

            using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var categories = await db.ServiceCategories
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var definitions = await db.ServiceDefinitions
                .AsNoTracking()
                .Where(d => d.IsActive)
                .ToListAsync(cancellationToken);

            // Group definitions by category
            var grouped = definitions
                .GroupBy(d => d.CategoryId)
                .Select(g =>
                {
                    var category = categories.FirstOrDefault(c => c.Id == g.Key);
                    return new
                    {
                        CategoryId = g.Key,
                        CategoryName = category?.Name ?? "Unknown",
                        ServiceGroup = NormalizeName(category?.Name ?? "other"),
                        Definitions = g.ToList()
                    };
                })
                .ToList();

            var basePath = GetKnowledgeBasePath();

            foreach (var group in grouped)
            {
                var groupFolder = group.ServiceGroup.ToLower().Replace(" ", "_");

                // Write services JSON
                var servicesDir = Path.Combine(basePath, "services", groupFolder);
                Directory.CreateDirectory(servicesDir);

                var serviceJson = new
                {
                    serviceGroup = group.ServiceGroup.ToUpper(),
                    subCategoryId = group.CategoryId.ToString(),
                    name = group.CategoryName,
                    cases = group.Definitions.Select(d => new
                    {
                        caseId = d.Id.ToString(),
                        name = d.Name,
                        description = d.Description
                    })
                };

                var servicesFilePath = Path.Combine(servicesDir, $"{groupFolder}.json");
                var servicesContent = JsonSerializer.Serialize(serviceJson, _jsonOptions);
                await File.WriteAllTextAsync(servicesFilePath, servicesContent, cancellationToken);

                // Write pricing JSON
                var pricingDir = Path.Combine(basePath, "pricing", groupFolder);
                Directory.CreateDirectory(pricingDir);

                var pricingJson = new
                {
                    subCategoryId = group.CategoryId.ToString(),
                    pricingByService = group.Definitions.Select(d => new
                    {
                        serviceId = d.Id.ToString(),
                        serviceName = d.Name,
                        basePrice = new { amount = d.BasePrice, currency = "VND" },
                        estimatedDuration = d.EstimatedDuration
                    })
                };

                var pricingFilePath = Path.Combine(pricingDir, $"{groupFolder}_pricing.json");
                var pricingContent = JsonSerializer.Serialize(pricingJson, _jsonOptions);
                await File.WriteAllTextAsync(pricingFilePath, pricingContent, cancellationToken);

                _logger.LogInformation("Synced KnowledgeBase for category '{CategoryName}' ({Count} services)",
                    group.CategoryName, group.Definitions.Count);
            }

            _logger.LogInformation("KnowledgeBase sync completed. {GroupCount} categories, {TotalCount} services synced.",
                grouped.Count, definitions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync KnowledgeBase from DB");
        }
    }

    private string GetKnowledgeBasePath()
    {
        // Try project root first (development)
        var projectRoot = _env.ContentRootPath;
        var kbPath = Path.Combine(projectRoot, "KnowledgeBase");

        if (Directory.Exists(kbPath))
            return kbPath;

        // Try Infrastructure project path
        var infraPath = Path.Combine(
            Directory.GetParent(projectRoot)?.FullName ?? projectRoot,
            "SmartService.Infrastructure",
            "KnowledgeBase");

        if (Directory.Exists(infraPath))
            return infraPath;

        // Fallback: create in content root
        Directory.CreateDirectory(kbPath);
        return kbPath;
    }

    private static string NormalizeName(string name)
    {
        // Convert Vietnamese names to simple folder-safe names
        return name
            .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
            .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
            .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
            .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
            .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
            .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
            .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
            .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
            .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
            .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
            .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y")
            .Replace("đ", "d").Replace("Đ", "D")
            .Replace(" ", "_")
            .ToLower();
    }
}
