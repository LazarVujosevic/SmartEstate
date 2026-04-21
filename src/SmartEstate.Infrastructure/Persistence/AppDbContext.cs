using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Domain.Entities;
using SmartEstate.Infrastructure.Identity;

namespace SmartEstate.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<MatchReport> MatchReports => Set<MatchReport>();
    public DbSet<FsboLead> FsboLeads => Set<FsboLead>();
    public DbSet<InAppNotification> Notifications => Set<InAppNotification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global tenant filters — automatically scope all tenant queries
        var tenantId = tenantContext.TenantId;
        if (tenantId.HasValue)
        {
            builder.Entity<Buyer>().HasQueryFilter(e => e.TenantId == tenantId.Value);
            builder.Entity<Property>().HasQueryFilter(e => e.TenantId == tenantId.Value);
            builder.Entity<MatchReport>().HasQueryFilter(e => e.TenantId == tenantId.Value);
            builder.Entity<InAppNotification>().HasQueryFilter(e => e.TenantId == tenantId.Value);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
