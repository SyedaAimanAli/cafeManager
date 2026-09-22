using RollUp.Core.Enums;

namespace RollUp.Application.DTOs;

public class UserListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsActive { get; set; }
    public int? OutletId { get; set; }
    public string OutletName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateUserRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.Cashier;
    public int? OutletId { get; set; }
}

public class UpdateUserRequestDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public int? OutletId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ResetPasswordDto
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

public class TenantBrandingDto
{
    public string BusinessName { get; set; } = string.Empty;
    public string? Tagline { get; set; }
    public string? LogoUrl { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string ThemeTemplate { get; set; } = "bistro";
    public string ColorScheme { get; set; } = "espresso";
    public string FontFamily { get; set; } = "inter";
    public string? CustomPrimaryColor { get; set; }
    public string? CustomAccentColor { get; set; }
}
