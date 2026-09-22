using System.ComponentModel.DataAnnotations;

namespace RollUp.Application.DTOs;

public class VendorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int OutletCount { get; set; }
}

public class CreateVendorDto
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
