namespace RollUp.Core.Entities;

public class OrderItem : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string SelectedVariant { get; set; } = string.Empty;
    public string SelectedAddons { get; set; } = string.Empty; // comma-separated
    public string SpecialInstructions { get; set; } = string.Empty;

    public decimal LineTotal => UnitPrice * Quantity;

    // Navigation
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
}
