using System.ComponentModel.DataAnnotations;

namespace RollUp.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string SelectedVariant { get; set; } = string.Empty;
    public string SelectedAddons { get; set; } = string.Empty;
    public string SpecialInstructions { get; set; } = string.Empty;
    public decimal LineTotal { get; set; }
}

public class CreateOrderDto
{
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    public string TableNumber { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "DineIn";

    public int OutletId { get; set; }

    [Required, MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public string SelectedVariant { get; set; } = string.Empty;
    public string SelectedAddons { get; set; } = string.Empty;
    public string SpecialInstructions { get; set; } = string.Empty;
}

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
