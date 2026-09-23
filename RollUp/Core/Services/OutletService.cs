using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RollUp.Core.Entities;
using RollUp.Core.Enums;
using RollUp.Core.Interfaces;
using RollUp.Infrastructure.Persistence;

namespace RollUp.Core.Services;

public class OutletService : IOutletService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public int? ActiveOutletId { get; private set; }
    public Outlet? ActiveOutlet { get; private set; }
    public event Action? OnActiveOutletChanged;

    public OutletService(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    private async Task<int> ResolveTenantIdAsync()
    {
        if (_tenantContext.CurrentTenantId.HasValue && _tenantContext.CurrentTenantId.Value > 0)
        {
            return _tenantContext.CurrentTenantId.Value;
        }

        if (!string.IsNullOrWhiteSpace(_tenantContext.CurrentTenantSlug))
        {
            var t = await _dbContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Slug == _tenantContext.CurrentTenantSlug);
            if (t != null) return t.Id;
        }

        // Fallback to latest active tenant
        var latest = await _dbContext.Tenants.IgnoreQueryFilters()
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync();

        return latest?.Id ?? 1;
    }

    public async Task InitializeActiveOutletAsync()
    {
        var outlets = await GetOutletsAsync();

        // If current active outlet does not belong to this tenant, reset it
        if (ActiveOutlet != null && !outlets.Any(o => o.Id == ActiveOutlet.Id))
        {
            ActiveOutletId = null;
            ActiveOutlet = null;
        }

        if (ActiveOutletId.HasValue && ActiveOutlet != null) return;

        if (outlets.Any())
        {
            var first = outlets.FirstOrDefault(o => o.IsActive) ?? outlets.First();
            ActiveOutletId = first.Id;
            ActiveOutlet = first;
            OnActiveOutletChanged?.Invoke();
        }
    }

    public async Task<List<Outlet>> GetOutletsAsync()
    {
        var tenantId = await ResolveTenantIdAsync();
        return await _dbContext.Outlets
            .IgnoreQueryFilters()
            .Where(o => !o.IsDeleted && o.TenantId == tenantId)
            .OrderBy(o => o.Id)
            .ToListAsync();
    }

    public async Task<Outlet?> GetOutletByIdAsync(int id)
    {
        var tenantId = await ResolveTenantIdAsync();
        return await _dbContext.Outlets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId && !o.IsDeleted);
    }

    public async Task<Outlet> CreateOutletAsync(string name, OutletType type, string address, string phone)
    {
        var tenantId = await ResolveTenantIdAsync();

        var outlet = new Outlet
        {
            Name = name.Trim(),
            OutletType = type,
            Address = address.Trim(),
            Phone = phone.Trim(),
            TenantId = tenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Outlets.AddAsync(outlet);
        await _dbContext.SaveChangesAsync();

        // If this is the only outlet or active outlet was not set, set it
        if (!ActiveOutletId.HasValue)
        {
            ActiveOutletId = outlet.Id;
            ActiveOutlet = outlet;
            OnActiveOutletChanged?.Invoke();
        }

        return outlet;
    }

    public async Task<Outlet?> UpdateOutletAsync(int id, string name, OutletType type, string address, string phone, bool isActive)
    {
        var outlet = await _dbContext.Outlets.FirstOrDefaultAsync(o => o.Id == id);
        if (outlet == null) return null;

        outlet.Name = name.Trim();
        outlet.OutletType = type;
        outlet.Address = address.Trim();
        outlet.Phone = phone.Trim();
        outlet.IsActive = isActive;
        outlet.UpdatedAt = DateTime.UtcNow;

        _dbContext.Outlets.Update(outlet);
        await _dbContext.SaveChangesAsync();

        if (ActiveOutletId == id)
        {
            ActiveOutlet = outlet;
            OnActiveOutletChanged?.Invoke();
        }

        return outlet;
    }

    public async Task SetActiveOutletAsync(int outletId)
    {
        if (ActiveOutletId == outletId && ActiveOutlet != null) return;

        var outlet = await _dbContext.Outlets.FirstOrDefaultAsync(o => o.Id == outletId);
        if (outlet != null)
        {
            ActiveOutletId = outlet.Id;
            ActiveOutlet = outlet;
            OnActiveOutletChanged?.Invoke();
        }
    }

    public async Task<OutletSummary?> GetOutletSummaryAsync(int outletId)
    {
        var outlet = await _dbContext.Outlets.FirstOrDefaultAsync(o => o.Id == outletId);
        if (outlet == null) return null;

        var today = DateTime.UtcNow.Date;

        var orders = await _dbContext.Orders
            .Where(o => o.OutletId == outletId)
            .Include(o => o.Items)
            .ToListAsync();

        var activeQueue = orders.Count(o =>
            o.Status == OrderStatus.Pending ||
            o.Status == OrderStatus.Preparing ||
            o.Status == OrderStatus.Ready);

        var todayOrders = orders.Where(o => o.CreatedAt >= today && o.Status != OrderStatus.Cancelled).ToList();
        var todayRevenue = todayOrders.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice));

        var staffCount = await _dbContext.Users.CountAsync(u => u.OutletId == outletId);
        var menuItemsCount = await _dbContext.MenuItems.CountAsync(m => m.OutletId == outletId || m.OutletId == 0);

        return new OutletSummary
        {
            OutletId = outlet.Id,
            Name = outlet.Name,
            Address = outlet.Address,
            Phone = outlet.Phone,
            OutletType = outlet.OutletType,
            IsActive = outlet.IsActive,
            ActiveQueueCount = activeQueue,
            TodayOrdersCount = todayOrders.Count,
            TodayRevenue = todayRevenue,
            TotalMenuItems = menuItemsCount,
            StaffCount = staffCount
        };
    }
}
