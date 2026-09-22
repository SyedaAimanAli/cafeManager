namespace RollUp.Core.Enums;

public enum Role
{
    Admin,
    Manager,
    Kitchen,
    Cashier,
    Customer
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

public enum OrderStatus
{
    Pending,
    Preparing,
    Ready,
    Completed,
    Cancelled
}

public enum OrderType
{
    DineIn,
    TakeAway
}
