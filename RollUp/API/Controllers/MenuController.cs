using RollUp.Application.DTOs;
using RollUp.Core.Entities;
using RollUp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RollUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly IRepository<Core.Entities.MenuItem> _items;
    private readonly IRepository<Category> _categories;

    public MenuController(
        IRepository<Core.Entities.MenuItem> items,
        IRepository<Category> categories)
    {
        _items      = items;
        _categories = categories;
    }

    /// <summary>Get all menu items for an outlet.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] int outletId)
    {
        var items = outletId > 0
            ? await _items.FindAsync(m => m.OutletId == outletId)
            : await _items.GetAllAsync();

        return Ok(items.Select(MapToDto));
    }

    /// <summary>Get a single menu item.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _items.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(MapToDto(item));
    }

    /// <summary>Create a new menu item.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateMenuItemDto dto)
    {
        var item = new Core.Entities.MenuItem
        {
            Name        = dto.Name,
            Description = dto.Description,
            Price       = dto.Price,
            ImageUrl    = dto.ImageUrl,
            IsAvailable = dto.IsAvailable,
            IsPopular   = dto.IsPopular,
            Tags        = dto.Tags,
            CategoryId  = dto.CategoryId,
            OutletId    = dto.OutletId
        };
        await _items.AddAsync(item);
        await _items.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToDto(item));
    }

    /// <summary>Update an existing menu item.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateMenuItemDto dto)
    {
        var item = await _items.GetByIdAsync(id);
        if (item is null) return NotFound();

        item.Name        = dto.Name;
        item.Description = dto.Description;
        item.Price       = dto.Price;
        item.ImageUrl    = dto.ImageUrl;
        item.IsAvailable = dto.IsAvailable;
        item.IsPopular   = dto.IsPopular;
        item.Tags        = dto.Tags;
        item.CategoryId  = dto.CategoryId;

        _items.Update(item);
        await _items.SaveChangesAsync();
        return Ok(MapToDto(item));
    }

    /// <summary>Toggle item availability.</summary>
    [HttpPatch("{id:int}/availability")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> ToggleAvailability(int id)
    {
        var item = await _items.GetByIdAsync(id);
        if (item is null) return NotFound();

        item.IsAvailable = !item.IsAvailable;
        _items.Update(item);
        await _items.SaveChangesAsync();
        return Ok(new { item.Id, item.IsAvailable });
    }

    /// <summary>Soft-delete a menu item.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _items.GetByIdAsync(id);
        if (item is null) return NotFound();

        item.IsDeleted = true;
        _items.Update(item);
        await _items.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Get all categories.</summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var cats = await _categories.GetAllAsync();
        return Ok(cats.Select(c => new { c.Id, c.Name, c.SortOrder }));
    }

    private static MenuItemDto MapToDto(Core.Entities.MenuItem m) => new()
    {
        Id           = m.Id,
        Name         = m.Name,
        Description  = m.Description,
        Price        = m.Price,
        ImageUrl     = m.ImageUrl,
        IsAvailable  = m.IsAvailable,
        IsPopular    = m.IsPopular,
        Tags         = m.Tags,
        CategoryId   = m.CategoryId,
        CategoryName = m.Category?.Name ?? string.Empty
    };
}
