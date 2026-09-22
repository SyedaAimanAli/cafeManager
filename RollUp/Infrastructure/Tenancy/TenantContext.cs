using RollUp.Core.Interfaces;

namespace RollUp.Infrastructure.Tenancy;

public class TenantContext : ITenantContext
{
    public int? CurrentTenantId { get; private set; }
    public string? CurrentTenantSlug { get; private set; }
    public string? CurrentTenantName { get; private set; }

    public void SetTenant(int tenantId, string? slug = null, string? name = null)
    {
        CurrentTenantId = tenantId;
        CurrentTenantSlug = slug;
        CurrentTenantName = name;
    }

    public void ClearTenant()
    {
        CurrentTenantId = null;
        CurrentTenantSlug = null;
        CurrentTenantName = null;
    }
}
