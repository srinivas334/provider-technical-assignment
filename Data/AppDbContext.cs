using Microsoft.EntityFrameworkCore;
using ProviderAssignmentStarter.Models;

namespace ProviderAssignmentStarter.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Provider> Providers { get; set; }
    public DbSet<License> Licenses { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter: standard queries automatically exclude soft-deleted providers.
        // Use IgnoreQueryFilters() on any query that needs audit/admin access to deleted records.
        modelBuilder.Entity<Provider>()
            .HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasKey(p => p.ProviderId);
            entity.Property(p => p.ProviderName).IsRequired().HasMaxLength(200);
            entity.Property(p => p.County).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Status).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => new { p.ProviderName, p.County })
                  .IsUnique()
                  .HasFilter("\"IsDeleted\" = 0");
            entity.HasIndex(p => p.IsDeleted);
        });

        modelBuilder.Entity<License>(entity =>
        {
            entity.HasKey(l => l.LicenseId);
            entity.Property(l => l.LicenseNumber).IsRequired().HasMaxLength(100);
            entity.Property(l => l.LicenseStatus).IsRequired().HasMaxLength(50);
            entity.Ignore(l => l.IsExpired);
            entity.HasIndex(l => l.LicenseNumber).IsUnique();

            entity.HasOne(l => l.Provider)
                  .WithMany(p => p.Licenses)
                  .HasForeignKey(l => l.ProviderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.AuditLogId);
            entity.Property(a => a.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Action).IsRequired().HasMaxLength(50);
            entity.Property(a => a.ChangeSummary).IsRequired().HasMaxLength(1000);
            entity.Property(a => a.PerformedBy).HasMaxLength(100);
            entity.HasIndex(a => new { a.EntityType, a.EntityId });
            entity.HasIndex(a => a.PerformedAt);
        });

        SeedTestData(modelBuilder);
    }

    protected virtual void SeedTestData(ModelBuilder modelBuilder) => SeedData(modelBuilder);

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var providers = new List<Provider>
        {
            new() { ProviderId = 1, ProviderName = "Fulton Family Care", County = "Fulton", Status = "Active", CreatedDate = new DateTime(2023, 1, 15, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new() { ProviderId = 2, ProviderName = "DeKalb Medical Group", County = "DeKalb", Status = "Active", CreatedDate = new DateTime(2023, 3, 22, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new() { ProviderId = 3, ProviderName = "Gwinnett Health Services", County = "Gwinnett", Status = "Active", CreatedDate = new DateTime(2023, 6, 10, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new() { ProviderId = 4, ProviderName = "Cobb County Clinics", County = "Cobb", Status = "Inactive", CreatedDate = new DateTime(2022, 11, 5, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new() { ProviderId = 5, ProviderName = "Clayton Care Partners", County = "Clayton", Status = "Pending", CreatedDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new() { ProviderId = 6, ProviderName = "Cherokee Wellness Center", County = "Cherokee", Status = "Active", CreatedDate = new DateTime(2022, 8, 19, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new() { ProviderId = 7, ProviderName = "Henry County Health", County = "Henry", Status = "Active", CreatedDate = new DateTime(2023, 9, 3, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            // Soft-deleted provider — remains in DB for audit, excluded from standard views
            new() { ProviderId = 8, ProviderName = "Archived Provider Inc", County = "Fulton", Status = "Inactive", CreatedDate = new DateTime(2021, 5, 14, 0, 0, 0, DateTimeKind.Utc), IsDeleted = true, DeletedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new() { ProviderId = 9, ProviderName = "Douglas Diagnostic Center", County = "Douglas", Status = "Active", CreatedDate = new DateTime(2024, 4, 7, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new() { ProviderId = 10, ProviderName = "Forsyth Primary Care", County = "Forsyth", Status = "Pending", CreatedDate = new DateTime(2024, 5, 20, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
        };

        modelBuilder.Entity<Provider>().HasData(providers);

        var licenses = new List<License>
        {
            // Fulton Family Care — Active provider, active license
            new() { LicenseId = 1, ProviderId = 1, LicenseNumber = "LIC-2023-0001", LicenseStatus = "Active", ExpirationDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
            new() { LicenseId = 2, ProviderId = 1, LicenseNumber = "LIC-2023-0002", LicenseStatus = "Suspended", ExpirationDate = new DateTime(2025, 6, 30, 0, 0, 0, DateTimeKind.Utc) },

            // DeKalb Medical Group — Active provider, expired license (scenario: active provider, expired license)
            new() { LicenseId = 3, ProviderId = 2, LicenseNumber = "LIC-2023-0010", LicenseStatus = "Expired", ExpirationDate = new DateTime(2024, 3, 22, 0, 0, 0, DateTimeKind.Utc) },
            new() { LicenseId = 4, ProviderId = 2, LicenseNumber = "LIC-2023-0011", LicenseStatus = "Active", ExpirationDate = new DateTime(2026, 3, 22, 0, 0, 0, DateTimeKind.Utc) },

            // Gwinnett — Active provider, one expired license
            new() { LicenseId = 5, ProviderId = 3, LicenseNumber = "LIC-2023-0020", LicenseStatus = "Active", ExpirationDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc) },
            new() { LicenseId = 6, ProviderId = 3, LicenseNumber = "LIC-2021-0021", LicenseStatus = "Expired", ExpirationDate = new DateTime(2023, 6, 10, 0, 0, 0, DateTimeKind.Utc) },

            // Cobb — Inactive provider, expired license
            new() { LicenseId = 7, ProviderId = 4, LicenseNumber = "LIC-2022-0030", LicenseStatus = "Expired", ExpirationDate = new DateTime(2023, 11, 5, 0, 0, 0, DateTimeKind.Utc) },

            // Clayton — Pending provider, active license
            new() { LicenseId = 8, ProviderId = 5, LicenseNumber = "LIC-2024-0040", LicenseStatus = "Active", ExpirationDate = new DateTime(2027, 2, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Cherokee — Active provider, active license
            new() { LicenseId = 9, ProviderId = 6, LicenseNumber = "LIC-2022-0050", LicenseStatus = "Active", ExpirationDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc) },

            // Henry — Active provider, expired license (scenario: active provider, expired license)
            new() { LicenseId = 10, ProviderId = 7, LicenseNumber = "LIC-2023-0060", LicenseStatus = "Expired", ExpirationDate = new DateTime(2024, 9, 3, 0, 0, 0, DateTimeKind.Utc) },

            // Archived Provider (soft-deleted) — license remains for audit
            new() { LicenseId = 11, ProviderId = 8, LicenseNumber = "LIC-2021-0070", LicenseStatus = "Expired", ExpirationDate = new DateTime(2022, 5, 14, 0, 0, 0, DateTimeKind.Utc) },

            // Douglas — Active provider, active license
            new() { LicenseId = 12, ProviderId = 9, LicenseNumber = "LIC-2024-0080", LicenseStatus = "Active", ExpirationDate = new DateTime(2027, 4, 7, 0, 0, 0, DateTimeKind.Utc) },

            // Forsyth — Pending, no active license yet
            new() { LicenseId = 13, ProviderId = 10, LicenseNumber = "LIC-2024-0090", LicenseStatus = "Active", ExpirationDate = new DateTime(2027, 5, 20, 0, 0, 0, DateTimeKind.Utc) },
        };

        modelBuilder.Entity<License>().HasData(licenses);
    }
}
