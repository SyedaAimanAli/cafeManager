using System.ComponentModel.DataAnnotations;

namespace RollUp.Application.DTOs;

public class MenuItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool IsPopular { get; set; }
    public string Tags { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}

public class CreateMenuItemDto
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required, Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public bool IsPopular { get; set; } = false;
    public string Tags { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int OutletId { get; set; }
}

public class QueueEntryDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int PartySize { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsSeated { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Position { get; set; }
}

public class CreateQueueEntryDto
{
    [Required, MinLength(2)]
    public string CustomerName { get; set; } = string.Empty;

    [Range(1, 50)]
    public int PartySize { get; set; } = 1;

    public string PhoneNumber { get; set; } = string.Empty;
}
