using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RollUp.Core.Interfaces;
using RollUp.Core.Models;
using RollUp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RollUp.Core.Services;

public class MenuService : IMenuService
{
    private readonly IRepository<RollUp.Core.Entities.MenuItem> _itemRepository;
    private readonly IRepository<RollUp.Core.Entities.Category> _categoryRepository;
    private readonly IRepository<RollUp.Core.Entities.Outlet> _outletRepository;
    private readonly AppDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IOutletService _outletService;

    public MenuService(
        IRepository<RollUp.Core.Entities.MenuItem> itemRepository,
        IRepository<RollUp.Core.Entities.Category> categoryRepository,
        IRepository<RollUp.Core.Entities.Outlet> outletRepository,
        AppDbContext context,
        ITenantContext tenantContext,
        IOutletService outletService)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
        _outletRepository = outletRepository;
        _context = context;
        _tenantContext = tenantContext;
        _outletService = outletService;
    }

    private async Task<int> ResolveTenantIdAsync()
    {
        if (_tenantContext.CurrentTenantId.HasValue && _tenantContext.CurrentTenantId.Value > 0)
        {
            return _tenantContext.CurrentTenantId.Value;
        }

        if (!string.IsNullOrWhiteSpace(_tenantContext.CurrentTenantSlug))
        {
            var t = await _context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Slug == _tenantContext.CurrentTenantSlug);
            if (t != null) return t.Id;
        }

        var latest = await _context.Tenants.IgnoreQueryFilters()
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync();

        return latest?.Id ?? 1;
    }

    public async Task<List<MenuItem>> GetAllItemsAsync(int? outletId = null)
    {
        var tenantId = await ResolveTenantIdAsync();
        var targetOutletId = outletId ?? _outletService.ActiveOutletId;

        var items = await _context.MenuItems
            .IgnoreQueryFilters()
            .Where(m => !m.IsDeleted && m.TenantId == tenantId)
            .ToListAsync();

        var categories = await _context.Categories
            .IgnoreQueryFilters()
            .Where(c => !c.IsDeleted && c.TenantId == tenantId)
            .ToListAsync();

        Dictionary<int, bool> branchAvailabilities = new();
        if (targetOutletId.HasValue && targetOutletId.Value > 0)
        {
            branchAvailabilities = await _context.OutletItemAvailabilities
                .Where(o => !o.IsDeleted && o.OutletId == targetOutletId.Value)
                .ToDictionaryAsync(o => o.MenuItemId, o => o.IsAvailable);
        }

        return items.Select(i =>
        {
            var m = MapToModel(i, categories);
            if (branchAvailabilities.TryGetValue(i.Id, out var isAvail))
            {
                m.IsAvailable = isAvail;
            }
            return m;
        }).ToList();
    }

    public async Task<List<MenuItem>> GetItemsByCategoryAsync(string categoryName, int? outletId = null)
    {
        var tenantId = await ResolveTenantIdAsync();
        var targetOutletId = outletId ?? _outletService.ActiveOutletId;

        var categories = await _context.Categories
            .IgnoreQueryFilters()
            .Where(c => !c.IsDeleted && c.TenantId == tenantId)
            .ToListAsync();

        var category = categories.FirstOrDefault(c => string.Equals(c.Name, categoryName, System.StringComparison.OrdinalIgnoreCase));
        if (category == null) return new List<MenuItem>();

        var items = await _context.MenuItems
            .IgnoreQueryFilters()
            .Where(i => !i.IsDeleted && i.TenantId == tenantId && i.CategoryId == category.Id)
            .ToListAsync();

        Dictionary<int, bool> branchAvailabilities = new();
        if (targetOutletId.HasValue && targetOutletId.Value > 0)
        {
            branchAvailabilities = await _context.OutletItemAvailabilities
                .Where(o => !o.IsDeleted && o.OutletId == targetOutletId.Value)
                .ToDictionaryAsync(o => o.MenuItemId, o => o.IsAvailable);
        }

        return items.Select(i =>
        {
            var m = MapToModel(i, categories);
            if (branchAvailabilities.TryGetValue(i.Id, out var isAvail))
            {
                m.IsAvailable = isAvail;
            }
            return m;
        }).ToList();
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var tenantId = await ResolveTenantIdAsync();
        return await _context.Categories
            .IgnoreQueryFilters()
            .Where(c => !c.IsDeleted && c.TenantId == tenantId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => c.Name)
            .ToListAsync();
    }

    public async Task<MenuItem?> GetItemByIdAsync(int id, int? outletId = null)
    {
        var tenantId = await ResolveTenantIdAsync();
        var targetOutletId = outletId ?? _outletService.ActiveOutletId;

        var entity = await _context.MenuItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId && !m.IsDeleted);
        if (entity == null) return null;
        
        var categories = await _context.Categories
            .IgnoreQueryFilters()
            .Where(c => !c.IsDeleted && c.TenantId == tenantId)
            .ToListAsync();

        var model = MapToModel(entity, categories);

        if (targetOutletId.HasValue && targetOutletId.Value > 0)
        {
            var branchRec = await _context.OutletItemAvailabilities
                .FirstOrDefaultAsync(o => !o.IsDeleted && o.OutletId == targetOutletId.Value && o.MenuItemId == id);
            if (branchRec != null)
            {
                model.IsAvailable = branchRec.IsAvailable;
            }
        }

        return model;
    }

    public async Task AddItemAsync(MenuItem model)
    {
        var tenantId = await ResolveTenantIdAsync();
        var categoryName = string.IsNullOrWhiteSpace(model.Category) ? "General" : model.Category.Trim();

        var category = await _context.Categories
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());

        if (category == null)
        {
            category = new RollUp.Core.Entities.Category 
            { 
                Name = categoryName,
                TenantId = tenantId,
                CreatedAt = System.DateTime.UtcNow
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        var outletId = model.OutletId > 0 ? model.OutletId : _outletService.ActiveOutletId;
        if (!outletId.HasValue || outletId.Value <= 0)
        {
            var outlet = await _context.Outlets
                .IgnoreQueryFilters()
                .Where(o => o.TenantId == tenantId && !o.IsDeleted)
                .OrderBy(o => o.Id)
                .FirstOrDefaultAsync();
            outletId = outlet?.Id ?? 1;
        }

        var entity = new RollUp.Core.Entities.MenuItem
        {
            Name = string.IsNullOrWhiteSpace(model.Name) ? "New Item" : model.Name.Trim(),
            Description = model.Description ?? string.Empty,
            Price = model.Price,
            ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) 
                ? "https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=400" 
                : model.ImageUrl.Trim(),
            IsAvailable = model.IsAvailable,
            IsPopular = model.IsPopular,
            Tags = model.Tags != null ? string.Join(",", model.Tags) : string.Empty,
            CategoryId = category.Id,
            OutletId = outletId.Value,
            TenantId = tenantId,
            CreatedAt = System.DateTime.UtcNow
        };

        await _context.MenuItems.AddAsync(entity);
        await _context.SaveChangesAsync();
        model.Id = entity.Id;
    }

    public async Task UpdateItemAsync(MenuItem model)
    {
        var tenantId = await ResolveTenantIdAsync();
        var entity = await _context.MenuItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == model.Id && m.TenantId == tenantId);
        if (entity == null) return;

        var categoryName = string.IsNullOrWhiteSpace(model.Category) ? "General" : model.Category.Trim();
        var category = await _context.Categories
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());

        if (category == null)
        {
            category = new RollUp.Core.Entities.Category 
            { 
                Name = categoryName,
                TenantId = tenantId,
                CreatedAt = System.DateTime.UtcNow
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        entity.Name = string.IsNullOrWhiteSpace(model.Name) ? entity.Name : model.Name.Trim();
        entity.Description = model.Description ?? string.Empty;
        entity.Price = model.Price;
        if (!string.IsNullOrWhiteSpace(model.ImageUrl)) entity.ImageUrl = model.ImageUrl.Trim();
        entity.IsAvailable = model.IsAvailable;
        entity.IsPopular = model.IsPopular;
        entity.Tags = model.Tags != null ? string.Join(",", model.Tags) : string.Empty;
        entity.CategoryId = category.Id;
        if (model.OutletId > 0)
        {
            entity.OutletId = model.OutletId;
        }
        entity.UpdatedAt = System.DateTime.UtcNow;

        _context.MenuItems.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int id)
    {
        var tenantId = await ResolveTenantIdAsync();
        var entity = await _context.MenuItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleAvailabilityAsync(int id, int? outletId = null)
    {
        var tenantId = await ResolveTenantIdAsync();
        var targetOutletId = outletId ?? _outletService.ActiveOutletId;

        var entity = await _context.MenuItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);
        if (entity == null) return;

        if (targetOutletId.HasValue && targetOutletId.Value > 0)
        {
            var branchRec = await _context.OutletItemAvailabilities
                .FirstOrDefaultAsync(o => o.OutletId == targetOutletId.Value && o.MenuItemId == id && !o.IsDeleted);

            if (branchRec != null)
            {
                branchRec.IsAvailable = !branchRec.IsAvailable;
                branchRec.UpdatedAt = System.DateTime.UtcNow;
                _context.OutletItemAvailabilities.Update(branchRec);
            }
            else
            {
                var newRec = new RollUp.Core.Entities.OutletMenuItemAvailability
                {
                    OutletId = targetOutletId.Value,
                    MenuItemId = id,
                    IsAvailable = !entity.IsAvailable,
                    CreatedAt = System.DateTime.UtcNow
                };
                await _context.OutletItemAvailabilities.AddAsync(newRec);
            }

            await _context.SaveChangesAsync();
        }
        else
        {
            entity.IsAvailable = !entity.IsAvailable;
            entity.UpdatedAt = System.DateTime.UtcNow;
            _context.MenuItems.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddCategoryAsync(string name)
    {
        var tenantId = await ResolveTenantIdAsync();
        var category = new RollUp.Core.Entities.Category 
        { 
            Name = name.Trim(),
            TenantId = tenantId,
            CreatedAt = System.DateTime.UtcNow
        };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(string name)
    {
        var tenantId = await ResolveTenantIdAsync();
        var category = await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Name == name && c.TenantId == tenantId);
        if (category != null)
        {
            category.IsDeleted = true;
            category.UpdatedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetArchivedCategoriesAsync()
    {
        var tenantId = await ResolveTenantIdAsync();
        return await _context.Categories
            .IgnoreQueryFilters()
            .Where(c => c.IsDeleted && c.TenantId == tenantId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => c.Name)
            .ToListAsync();
    }

    public async Task RestoreCategoryAsync(string name)
    {
        var tenantId = await ResolveTenantIdAsync();
        var category = await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Name == name && c.IsDeleted && c.TenantId == tenantId);

        if (category != null)
        {
            category.IsDeleted = false;
            category.UpdatedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task PermanentlyDeleteCategoryAsync(string name)
    {
        var tenantId = await ResolveTenantIdAsync();
        var category = await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Name == name && c.IsDeleted && c.TenantId == tenantId);

        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    private MenuItem MapToModel(RollUp.Core.Entities.MenuItem entity, IEnumerable<RollUp.Core.Entities.Category> categories)
    {
        return new MenuItem
        {
            Id = entity.Id,
            OutletId = entity.OutletId,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            ImageUrl = entity.ImageUrl,
            IsAvailable = entity.IsAvailable,
            IsPopular = entity.IsPopular,
            Tags = string.IsNullOrWhiteSpace(entity.Tags) 
                ? new List<string>() 
                : entity.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            Category = categories.FirstOrDefault(c => c.Id == entity.CategoryId)?.Name ?? "Uncategorized"
        };
    }
}
