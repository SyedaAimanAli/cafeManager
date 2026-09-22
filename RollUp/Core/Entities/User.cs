using RollUp.Core.Enums;
using RollUp.Core.Interfaces;

namespace RollUp.Core.Entities;

public class User : BaseEntity, ITenantEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.Cashier;
    public bool IsActive { get; set; } = true;

    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Navigation
    public int? OutletId { get; set; }
    public Outlet? Outlet { get; set; }
}
