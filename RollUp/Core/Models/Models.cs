using RollUp.Core.Enums;

namespace RollUp.Core.Models;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public bool IsPopular { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<MenuItemVariant> Variants { get; set; } = new();
    public List<MenuItemAddon> Addons { get; set; } = new();
}

public class MenuItemVariant
{
    public string Name { get; set; } = string.Empty;
    public decimal PriceModifier { get; set; }
}

public class MenuItemAddon
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class CartItem
{
    public MenuItem MenuItem { get; set; } = new();
    public int Quantity { get; set; } = 1;
    public string SelectedVariant { get; set; } = string.Empty;
    public List<string> SelectedAddons { get; set; } = new();
    public string SpecialInstructions { get; set; } = string.Empty;

    public decimal TotalPrice
    {
        get
        {
            var basePrice = MenuItem.Price;
            var variant = MenuItem.Variants.FirstOrDefault(v => v.Name == SelectedVariant);
            if (variant != null) basePrice += variant.PriceModifier;
            
            var addonsPrice = MenuItem.Addons
                .Where(a => SelectedAddons.Contains(a.Name))
                .Sum(a => a.Price);
            
            return (basePrice + addonsPrice) * Quantity;
        }
    }
}

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = new();
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public string TableNumber { get; set; } = string.Empty;
    public OrderType Type { get; set; } = OrderType.DineIn;
}


