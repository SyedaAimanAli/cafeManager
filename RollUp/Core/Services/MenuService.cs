using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RollUp.Core.Interfaces;
using RollUp.Core.Models;

namespace RollUp.Core.Services;

public class MenuService : IMenuService
{
    private readonly IRepository<RollUp.Core.Entities.MenuItem> _itemRepository;
    private readonly IRepository<RollUp.Core.Entities.Category> _categoryRepository;
    private readonly IRepository<RollUp.Core.Entities.Outlet> _outletRepository;

    public MenuService(
        IRepository<RollUp.Core.Entities.MenuItem> itemRepository,
        IRepository<RollUp.Core.Entities.Category> categoryRepository,
        IRepository<RollUp.Core.Entities.Outlet> outletRepository)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
        _outletRepository = outletRepository;
    }

    public async Task<List<MenuItem>> GetAllItemsAsync()
    {
        var items = await _itemRepository.GetAllAsync();
        var categories = await _categoryRepository.GetAllAsync();
        
        return items.Select(i => MapToModel(i, categories)).ToList();
    }

    public async Task<List<MenuItem>> GetItemsByCategoryAsync(string categoryName)
    {
        var categories = await _categoryRepository.GetAllAsync();
        var category = categories.FirstOrDefault(c => c.Name == categoryName);
        if (category == null) return new List<MenuItem>();

        var items = await _itemRepository.FindAsync(i => i.CategoryId == category.Id);
        return items.Select(i => MapToModel(i, categories)).ToList();
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => c.Name).ToList();
    }

    public async Task<MenuItem?> GetItemByIdAsync(int id)
    {
        var entity = await _itemRepository.GetByIdAsync(id);
        if (entity == null) return null;
        
        var categories = await _categoryRepository.GetAllAsync();
        return MapToModel(entity, categories);
    }

    public async Task AddItemAsync(MenuItem model)
    {
        var categories = await _categoryRepository.GetAllAsync();
        var category = categories.FirstOrDefault(c => c.Name == model.Category);
        if (category == null)
        {
            category = new RollUp.Core.Entities.Category { Name = model.Category };
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();
        }

        var outlets = await _outletRepository.GetAllAsync();
        var defaultOutletId = outlets.FirstOrDefault()?.Id ?? 1;

        var entity = new RollUp.Core.Entities.MenuItem
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            ImageUrl = model.ImageUrl,
            IsAvailable = model.IsAvailable,
            IsPopular = model.IsPopular,
            Tags = string.Join(",", model.Tags),
            CategoryId = category.Id,
            OutletId = defaultOutletId
        };

        await _itemRepository.AddAsync(entity);
        await _itemRepository.SaveChangesAsync();
        model.Id = entity.Id;
    }

    public async Task UpdateItemAsync(MenuItem model)
    {
        var entity = await _itemRepository.GetByIdAsync(model.Id);
        if (entity == null) return;

        var categories = await _categoryRepository.GetAllAsync();
        var category = categories.FirstOrDefault(c => c.Name == model.Category);
        
        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.Price = model.Price;
        entity.ImageUrl = model.ImageUrl;
        entity.IsAvailable = model.IsAvailable;
        entity.IsPopular = model.IsPopular;
        entity.Tags = string.Join(",", model.Tags);
        if (category != null) entity.CategoryId = category.Id;

        _itemRepository.Update(entity);
        await _itemRepository.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int id)
    {
        var entity = await _itemRepository.GetByIdAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            _itemRepository.Update(entity);
            await _itemRepository.SaveChangesAsync();
        }
    }

    public async Task ToggleAvailabilityAsync(int id)
    {
        var entity = await _itemRepository.GetByIdAsync(id);
        if (entity != null)
        {
            entity.IsAvailable = !entity.IsAvailable;
            _itemRepository.Update(entity);
            await _itemRepository.SaveChangesAsync();
        }
    }

    public async Task AddCategoryAsync(string name)
    {
        var category = new RollUp.Core.Entities.Category { Name = name };
        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(string name)
    {
        var categories = await _categoryRepository.GetAllAsync();
        var category = categories.FirstOrDefault(c => c.Name == name);
        if (category != null)
        {
            category.IsDeleted = true;
            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();
        }
    }

    private MenuItem MapToModel(RollUp.Core.Entities.MenuItem entity, IEnumerable<RollUp.Core.Entities.Category> categories)
    {
        return new MenuItem
        {
            Id = entity.Id,
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
