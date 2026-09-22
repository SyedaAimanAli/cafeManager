using RollUp.Core.Interfaces;

namespace RollUp.Core.Entities;

public class Category : BaseEntity, ITenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;

    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Navigation
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
