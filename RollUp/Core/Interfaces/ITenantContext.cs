namespace RollUp.Core.Interfaces;

/// <summary>
/// Provides access to the current tenant context during request/circuit execution.
/// </summary>
public interface ITenantContext
{
    int? CurrentTenantId { get; }
    string? CurrentTenantSlug { get; }
    string? CurrentTenantName { get; }
    void SetTenant(int tenantId, string? slug = null, string? name = null);
    void ClearTenant();
}
