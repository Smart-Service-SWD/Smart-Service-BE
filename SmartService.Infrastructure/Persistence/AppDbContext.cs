using Microsoft.EntityFrameworkCore;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;

namespace SmartService.Infrastructure.Persistence;

public class AppDbContext : DbContext
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

            // ---- Value Object: ServiceComplexity
            entity.OwnsOne(x => x.Complexity, complexity =>
            {
                complexity.Property(c => c.Level)
                          .HasColumnName("ComplexityLevel")
                          .IsRequired();
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
    }
}
