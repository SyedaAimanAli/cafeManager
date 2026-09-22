namespace RollUp.Core.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Basic, Pro, Enterprise
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public int MaxOutlets { get; set; }
    public int MaxUsers { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
