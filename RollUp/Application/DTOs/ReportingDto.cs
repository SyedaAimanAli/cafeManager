using System;
using System.Collections.Generic;

namespace RollUp.Application.DTOs;

public class SalesReportSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int ActiveOrders { get; set; }
    public int DineInCount { get; set; }
    public int TakeawayCount { get; set; }

    public string TopSellingItemName { get; set; } = string.Empty;
    public int TopSellingItemUnits { get; set; }
    public decimal TopSellingItemRevenue { get; set; }

    public List<ProductSalesItemDto> ProductSales { get; set; } = new();
    public List<CategorySalesItemDto> CategorySales { get; set; } = new();
    public List<DailyRevenuePointDto> DailyRevenue { get; set; } = new();
    public List<HourlyTrafficPointDto> HourlyTraffic { get; set; } = new();
}

public class ProductSalesItemDto
{
    public int MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
    public decimal UnitPrice { get; set; }
    public double RevenuePercentage { get; set; }
}

public class CategorySalesItemDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
    public double RevenuePercentage { get; set; }
    public string ColorHex { get; set; } = "#3D2314";
}

public class DailyRevenuePointDto
{
    public DateTime Date { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrdersCount { get; set; }
}

public class HourlyTrafficPointDto
{
    public int Hour { get; set; }
    public string HourLabel { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal Revenue { get; set; }
}
