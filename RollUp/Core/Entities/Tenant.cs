namespace RollUp.Core.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // Unique URL-friendly slug (e.g. "rollup-cafe")
    public string? LogoUrl { get; set; } // Base64 or URL
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Tagline { get; set; }
    public bool IsActive { get; set; } = true;

    // Menu Design & Branding
    public string ThemeTemplate { get; set; } = "bistro";
    public string ColorScheme { get; set; } = "espresso";
    public string FontFamily { get; set; } = "inter";
    public string? CustomPrimaryColor { get; set; }
    public string? CustomAccentColor { get; set; }

    // Navigation
    public ICollection<Outlet> Outlets { get; set; } = new List<Outlet>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();
}
