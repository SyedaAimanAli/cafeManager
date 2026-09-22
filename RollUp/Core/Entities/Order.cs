using RollUp.Core.Enums;
using RollUp.Core.Interfaces;

namespace RollUp.Core.Entities;

public class Order : BaseEntity, ITenantEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public OrderType Type { get; set; } = OrderType.DineIn;
    public DateTime? CompletedAt { get; set; }

    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Navigation
    public int OutletId { get; set; }
    public Outlet Outlet { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
}
