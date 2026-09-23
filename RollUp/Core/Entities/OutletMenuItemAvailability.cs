namespace RollUp.Core.Entities;

public class OutletMenuItemAvailability : BaseEntity
{
    public int OutletId { get; set; }
    public Outlet Outlet { get; set; } = null!;

    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public bool IsAvailable { get; set; }
}
