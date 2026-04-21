using Microsoft.EntityFrameworkCore;
using SmartEstate.Domain.Entities;

namespace SmartEstate.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Buyer> Buyers { get; }
    DbSet<Property> Properties { get; }
    DbSet<MatchReport> MatchReports { get; }
    DbSet<FsboLead> FsboLeads { get; }
    DbSet<InAppNotification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
