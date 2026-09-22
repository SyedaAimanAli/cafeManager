using RollUp.Core.Enums;

namespace RollUp.Core.Entities;

public class Payment : BaseEntity
{
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty; // Cash, Card, Online
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionReference { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Navigation
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
