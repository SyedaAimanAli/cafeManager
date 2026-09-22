using System.ComponentModel.DataAnnotations;

namespace RollUp.Application.DTOs;

public class TenantOnboardingRequestDto
{
    // Business Details
    [Required(ErrorMessage = "Business name is required")]
    [MinLength(2, ErrorMessage = "Business name must be at least 2 characters")]
    public string BusinessName { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? LogoUrl { get; set; } // Base64 string or image URL

    [Required(ErrorMessage = "Country is required")]
    public string Country { get; set; } = "United States";

    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = string.Empty;

    public string Currency { get; set; } = "USD";

    public string? ContactPhone { get; set; }

    public string? Address { get; set; }

    public string? Tagline { get; set; }

    // Admin User Credentials
    [Required(ErrorMessage = "Admin full name is required")]
    public string AdminFullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admin email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string AdminEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string AdminPassword { get; set; } = string.Empty;
}

public class TenantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Tagline { get; set; }
    public bool IsActive { get; set; }
}

public class OnboardingResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public TenantDto Tenant { get; set; } = null!;
    public UserDto User { get; set; } = null!;
    public int OutletId { get; set; }
}
