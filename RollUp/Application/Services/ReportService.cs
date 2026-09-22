using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RollUp.Application.DTOs;
using RollUp.Core.Entities;
using RollUp.Core.Enums;
using RollUp.Core.Interfaces;
using RollUp.Infrastructure.Persistence;

namespace RollUp.Application.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ReportService(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<SalesReportSummaryDto> GetSalesReportAsync(string timeframe = "7days")
    {
        var tenantId = _tenantContext.CurrentTenantId ?? 1;
        var now = DateTime.UtcNow;

        DateTime? filterStart = timeframe switch
        {
            "today" => now.Date,
            "7days" => now.Date.AddDays(-6),
            "30days" => now.Date.AddDays(-29),
            _ => null
        };

        // Query orders with items
        var query = _db.Orders
            .IgnoreQueryFilters()
            .Where(o => !o.IsDeleted && o.TenantId == tenantId);

        if (filterStart.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= filterStart.Value);
        }

        var orders = await query
            .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
                    .ThenInclude(m => m.Category)
            .ToListAsync();

        var validOrders = orders.Where(o => o.Status != OrderStatus.Cancelled).ToList();

        var allItems = validOrders.SelectMany(o => o.Items).ToList();
        var totalRevenue = allItems.Sum(i => i.LineTotal);
        var totalOrdersCount = validOrders.Count;
        var aov = totalOrdersCount > 0 ? totalRevenue / totalOrdersCount : 0m;

        var completedOrdersCount = orders.Count(o => o.Status == OrderStatus.Completed);
        var cancelledOrdersCount = orders.Count(o => o.Status == OrderStatus.Cancelled);
        var activeOrdersCount = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Preparing || o.Status == OrderStatus.Ready);
        var dineInCount = validOrders.Count(o => o.Type == OrderType.DineIn);
        var takeawayCount = validOrders.Count(o => o.Type == OrderType.TakeAway);

        // Product-wise aggregation
        var productGroups = allItems
            .GroupBy(i => new { i.MenuItemId, i.MenuItem.Name, CategoryName = i.MenuItem.Category?.Name ?? "Uncategorized", i.MenuItem.ImageUrl, i.MenuItem.Price })
            .Select(g => new ProductSalesItemDto
            {
                MenuItemId = g.Key.MenuItemId,
                ItemName = g.Key.Name,
                CategoryName = g.Key.CategoryName,
                ImageUrl = g.Key.ImageUrl,
                UnitPrice = g.Key.Price,
                UnitsSold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal),
                RevenuePercentage = totalRevenue > 0 ? (double)(g.Sum(x => x.LineTotal) / totalRevenue * 100) : 0
            })
            .OrderByDescending(p => p.Revenue)
            .ToList();

        var topProduct = productGroups.FirstOrDefault();

        // Category-wise aggregation
        var categoryPalette = new[] { "#3D2314", "#10B981", "#DB2777", "#F59E0B", "#6366F1", "#EC4899", "#14B8A6" };
        var catIndex = 0;

        var categoryGroups = allItems
            .GroupBy(i => i.MenuItem.Category?.Name ?? "Uncategorized")
            .Select(g => new CategorySalesItemDto
            {
                CategoryName = g.Key,
                UnitsSold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal),
                RevenuePercentage = totalRevenue > 0 ? (double)(g.Sum(x => x.LineTotal) / totalRevenue * 100) : 0,
                ColorHex = categoryPalette[catIndex++ % categoryPalette.Length]
            })
            .OrderByDescending(c => c.Revenue)
            .ToList();

        // Daily Revenue Trajectory (last 7 or 30 days)
        var daysSpan = timeframe switch
        {
            "today" => 1,
            "7days" => 7,
            "30days" => 30,
            _ => 14
        };

        var dailyPoints = new List<DailyRevenuePointDto>();
        for (int i = daysSpan - 1; i >= 0; i--)
        {
            var dayDate = now.Date.AddDays(-i);
            var dayOrders = validOrders.Where(o => o.CreatedAt.Date == dayDate).ToList();
            var dayItems = dayOrders.SelectMany(o => o.Items).ToList();

            dailyPoints.Add(new DailyRevenuePointDto
            {
                Date = dayDate,
                DayLabel = timeframe == "today" ? "Today" : dayDate.ToString("ddd, MMM d", CultureInfo.InvariantCulture),
                Revenue = dayItems.Sum(x => x.LineTotal),
                OrdersCount = dayOrders.Count
            });
        }

        // Hourly Rush Traffic (0-23 hours)
        var hourlyPoints = new List<HourlyTrafficPointDto>();
        for (int h = 7; h <= 21; h++) // Common cafe operating hours 7 AM to 9 PM
        {
            var hourOrders = validOrders.Where(o => o.CreatedAt.Hour == h).ToList();
            var hourItems = hourOrders.SelectMany(o => o.Items).ToList();
            var timeFormatted = DateTime.Today.AddHours(h).ToString("h tt");

            hourlyPoints.Add(new HourlyTrafficPointDto
            {
                Hour = h,
                HourLabel = timeFormatted,
                OrdersCount = hourOrders.Count,
                Revenue = hourItems.Sum(x => x.LineTotal)
            });
        }

        return new SalesReportSummaryDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrdersCount,
            AverageOrderValue = aov,
            CompletedOrders = completedOrdersCount,
            CancelledOrders = cancelledOrdersCount,
            ActiveOrders = activeOrdersCount,
            DineInCount = dineInCount,
            TakeawayCount = takeawayCount,
            TopSellingItemName = topProduct?.ItemName ?? "N/A",
            TopSellingItemUnits = topProduct?.UnitsSold ?? 0,
            TopSellingItemRevenue = topProduct?.Revenue ?? 0m,
            ProductSales = productGroups,
            CategorySales = categoryGroups,
            DailyRevenue = dailyPoints,
            HourlyTraffic = hourlyPoints
        };
    }
}
