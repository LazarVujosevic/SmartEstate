using SmartEstate.Domain.Common;
using SmartEstate.Domain.Enums;

namespace SmartEstate.Domain.Entities;

public class FsboLead : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public PortalSource Source { get; set; }
    public string RawDescription { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? Price { get; set; }
    public FsboLeadStatus Status { get; set; } = FsboLeadStatus.New;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public bool NotificationsSent { get; set; } = false;
}
