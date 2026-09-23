using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RollUp.Core.Entities;
using RollUp.Core.Enums;

namespace RollUp.Core.Interfaces;

public class OutletSummary
{
    public int OutletId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public OutletType OutletType { get; set; }
    public bool IsActive { get; set; }
    public int ActiveQueueCount { get; set; }
    public int TodayOrdersCount { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TotalMenuItems { get; set; }
    public int StaffCount { get; set; }
}

public interface IOutletService
{
    int? ActiveOutletId { get; }
    Outlet? ActiveOutlet { get; }
    event Action? OnActiveOutletChanged;

    Task<List<Outlet>> GetOutletsAsync();
    Task<Outlet?> GetOutletByIdAsync(int id);
    Task<Outlet> CreateOutletAsync(string name, OutletType type, string address, string phone);
    Task<Outlet?> UpdateOutletAsync(int id, string name, OutletType type, string address, string phone, bool isActive);
    Task<OutletSummary?> GetOutletSummaryAsync(int outletId);
    Task SetActiveOutletAsync(int outletId);
    Task InitializeActiveOutletAsync();
}
