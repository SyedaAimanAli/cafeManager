namespace RollUp.Core.Interfaces;

/// <summary>
/// Marks an entity as belonging to a specific tenant for data isolation.
/// </summary>
public interface ITenantEntity
{
    public int TenantId { get; set; }
}
