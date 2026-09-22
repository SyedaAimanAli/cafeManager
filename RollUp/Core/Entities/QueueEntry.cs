using RollUp.Core.Interfaces;

namespace RollUp.Core.Entities;

public class QueueEntry : BaseEntity, ITenantEntity
{
    public string CustomerName { get; set; } = string.Empty;
    public int PartySize { get; set; } = 1;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsSeated { get; set; } = false;
    public DateTime? SeatedAt { get; set; }
    public int Position { get; set; }

    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Navigation
    public int OutletId { get; set; }
    public Outlet Outlet { get; set; } = null!;
}
