using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;
using SmartService.Infrastructure.Auth;

namespace SmartService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core implementation of IAppDbContext.
/// This is the concrete database context for the application,
/// responsible for mapping domain entities to the database schema.
/// </summary>
public class AppDbContext : DbContext, IAppDbContext
{
    // ========= ENTITIES =========
    public DbSet<User> Users => Set<User>();
    public DbSet<ServiceAgent> ServiceAgents => Set<ServiceAgent>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<ServiceAttachment> ServiceAttachments => Set<ServiceAttachment>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<MatchingResult> MatchingResults => Set<MatchingResult>();
    public DbSet<ServiceFeedback> ServiceFeedbacks => Set<ServiceFeedback>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<AgentCapability> AgentCapabilities => Set<AgentCapability>();
    public DbSet<ServiceAnalysis> ServiceAnalyses => Set<ServiceAnalysis>();
    public DbSet<ServiceDefinition> ServiceDefinitions => Set<ServiceDefinition>();
    public DbSet<AuthData> AuthData => Set<AuthData>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ==========================
        // USER
        // ==========================
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
            entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        });

        // ==========================
        // SERVICE AGENT
        // ==========================
        modelBuilder.Entity<ServiceAgent>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasMany(x => x.Capabilities)
                  .WithOne()
                  .HasForeignKey("ServiceAgentId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AgentCapability>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.OwnsOne(x => x.MaxComplexity, complexity =>
            {
                complexity.Property(c => c.Level)
                          .HasColumnName("MaxComplexityLevel")
                          .IsRequired();
            });
        });

        // ==========================
        // SERVICE CATEGORY
        // ==========================
        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(150);
        });

        // ==========================
        // SERVICE REQUEST (AGGREGATE ROOT)
        // ==========================
        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Description)
                  .HasMaxLength(1000);

            // ---- Value Object: ServiceComplexity (nullable - only set after Evaluate)
            entity.OwnsOne(x => x.Complexity, complexity =>
            {
                complexity.Property(c => c.Level)
                          .HasColumnName("ComplexityLevel");
                          // Note: Not required - Complexity is only set after Evaluate() is called
            });

            // ---- Value Object: Money
            entity.OwnsOne(x => x.EstimatedCost, money =>
            {
                money.Property(m => m.Amount)
                     .HasColumnName("EstimatedCost_Amount")
                     .HasPrecision(18, 2);

                money.Property(m => m.Currency)
                     .HasColumnName("EstimatedCost_Currency")
                     .HasMaxLength(10);
            });

            // ---- Value Object: ServiceStatus (Enum-like VO)
            entity.Property(x => x.Status)
                    .HasConversion<int>()
                            .IsRequired();


            entity.HasMany<ServiceAttachment>()
                .WithOne()
                .HasForeignKey(x => x.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany<MatchingResult>()
                .WithOne()
                .HasForeignKey(x => x.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        // ==========================
        // SERVICE ATTACHMENT
        // ==========================
        modelBuilder.Entity<ServiceAttachment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileUrl).IsRequired();
        });

        // ==========================
        // MATCHING RESULT
        // ==========================
        modelBuilder.Entity<MatchingResult>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServiceRequestId).IsRequired();
            entity.Property(x => x.ServiceAgentId).IsRequired();

            entity.Property(x => x.MatchingScore)
                .IsRequired();

            // ---- Value Object: ServiceComplexity
            entity.OwnsOne(x => x.SupportedComplexity, complexity =>
            {
                complexity.Property(c => c.Level)
                        .HasColumnName("SupportedComplexityLevel")
                        .IsRequired();
            });
        });

        // ==========================
        // ASSIGNMENT
        // ==========================
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServiceRequestId).IsRequired();

            // ---- Value Object: Money
            entity.OwnsOne(x => x.EstimatedCost, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("EstimatedCost_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("EstimatedCost_Currency")
                    .HasMaxLength(10)
                    .IsRequired();
            });
        });

        // ==========================
        // SERVICE FEEDBACK
        // ==========================
        modelBuilder.Entity<ServiceFeedback>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Rating).IsRequired();
        });

        // ==========================
        // ACTIVITY LOG (AUDIT)
        // ==========================
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).IsRequired();
        });

        // ==========================
        // SERVICE ANALYSIS (AI Results)
        // ==========================
        modelBuilder.Entity<ServiceAnalysis>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ServiceRequestId).IsRequired();
            entity.Property(x => x.ComplexityLevel).IsRequired();
            entity.Property(x => x.UrgencyLevel).IsRequired();
            entity.Property(x => x.SafetyAdvice).HasMaxLength(1000);
            entity.Property(x => x.Summary).HasMaxLength(2000);
            entity.HasIndex(x => x.ServiceRequestId).IsUnique();
        });

        // ==========================
        // SERVICE DEFINITION
        // ==========================
        modelBuilder.Entity<ServiceDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.BasePrice).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.EstimatedDuration).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasOne<ServiceCategory>()
                  .WithMany()
                  .HasForeignKey(x => x.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================
        // AUTH DATA (Infrastructure)
        // ==========================
        modelBuilder.Entity<AuthData>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(x => x.EncryptedRefreshToken).HasMaxLength(1000);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.UserId).IsUnique();
        });
    }
}
