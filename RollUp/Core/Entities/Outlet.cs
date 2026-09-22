using RollUp.Core.Interfaces;

namespace RollUp.Core.Entities;

public class Outlet : BaseEntity, ITenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Legacy Vendor link (optional/maintained)
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public ICollection<User> Staff { get; set; } = new List<User>();
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<QueueEntry> QueueEntries { get; set; } = new List<QueueEntry>();
}
