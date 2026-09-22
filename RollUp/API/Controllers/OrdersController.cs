using RollUp.Application.DTOs;
using RollUp.Core.Entities;
using RollUp.Core.Enums;
using RollUp.Core.Interfaces;
using RollUp.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace RollUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IRepository<Order> _orders;
    private readonly IRepository<OrderItem> _orderItems;
    private readonly IRepository<Core.Entities.MenuItem> _menuItems;
    private readonly IHubContext<OrderHub> _hub;

    public OrdersController(
        IRepository<Order> orders,
        IRepository<OrderItem> orderItems,
        IRepository<Core.Entities.MenuItem> menuItems,
        IHubContext<OrderHub> hub)
    {
        _orders     = orders;
        _orderItems = orderItems;
        _menuItems  = menuItems;
        _hub        = hub;
    }

    /// <summary>Get all orders (optionally filtered by outlet and/or status).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? outletId,
        [FromQuery] string? status)
    {
        var all = outletId.HasValue
            ? await _orders.FindAsync(o => o.OutletId == outletId.Value)
            : await _orders.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<OrderStatus>(status, true, out var parsed))
            all = all.Where(o => o.Status == parsed);

        return Ok(all.Select(MapToDto));
    }

    /// <summary>Get a single order by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orders.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(MapToDto(order));
    }

    /// <summary>Create a new order.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        if (!Enum.TryParse<OrderType>(dto.Type, true, out var orderType))
            return BadRequest(new { message = $"Unknown order type: {dto.Type}" });

        var order = new Order
        {
            OrderNumber  = GenerateOrderNumber(),
            CustomerName = dto.CustomerName,
            TableNumber  = dto.TableNumber,
            Type         = orderType,
            OutletId     = dto.OutletId,
            Status       = OrderStatus.Pending
        };

        foreach (var itemDto in dto.Items)
        {
            var menuItem = await _menuItems.GetByIdAsync(itemDto.MenuItemId);
            if (menuItem is null)
                return BadRequest(new { message = $"MenuItem {itemDto.MenuItemId} not found." });

            order.Items.Add(new OrderItem
            {
                MenuItemId          = itemDto.MenuItemId,
                Quantity            = itemDto.Quantity,
                UnitPrice           = menuItem.Price,
                SelectedVariant     = itemDto.SelectedVariant,
                SelectedAddons      = itemDto.SelectedAddons,
                SpecialInstructions = itemDto.SpecialInstructions
            });
        }

        await _orders.AddAsync(order);
        await _orders.SaveChangesAsync();

        await _hub.Clients.Group($"orders-{order.OutletId}")
            .SendAsync("OrderCreated", order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapToDto(order));
    }

    /// <summary>Update an order's status.</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
            return BadRequest(new { message = $"Unknown status: {dto.Status}" });

        var order = await _orders.GetByIdAsync(id);
        if (order is null) return NotFound();

        order.Status = newStatus;
        if (newStatus == OrderStatus.Completed)
            order.CompletedAt = DateTime.UtcNow;

        _orders.Update(order);
        await _orders.SaveChangesAsync();

        await _hub.Clients.Group($"orders-{order.OutletId}")
            .SendAsync("OrderStatusChanged", new { order.Id, Status = order.Status.ToString() });

        return Ok(MapToDto(order));
    }

    /// <summary>Cancel an order.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _orders.GetByIdAsync(id);
        if (order is null) return NotFound();

        order.Status = OrderStatus.Cancelled;
        _orders.Update(order);
        await _orders.SaveChangesAsync();

        await _hub.Clients.Group($"orders-{order.OutletId}")
            .SendAsync("OrderStatusChanged", new { order.Id, Status = "Cancelled" });

        return NoContent();
    }

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

    private static OrderDto MapToDto(Order o) => new()
    {
        Id           = o.Id,
        OrderNumber  = o.OrderNumber,
        CustomerName = o.CustomerName,
        TableNumber  = o.TableNumber,
        Status       = o.Status.ToString(),
        Type         = o.Type.ToString(),
        CreatedAt    = o.CreatedAt,
        CompletedAt  = o.CompletedAt,
        TotalAmount  = o.Items.Sum(i => i.UnitPrice * i.Quantity),
        Items        = o.Items.Select(i => new OrderItemDto
        {
            MenuItemId          = i.MenuItemId,
            MenuItemName        = i.MenuItem?.Name ?? string.Empty,
            Quantity            = i.Quantity,
            UnitPrice           = i.UnitPrice,
            SelectedVariant     = i.SelectedVariant,
            SelectedAddons      = i.SelectedAddons,
            SpecialInstructions = i.SpecialInstructions,
            LineTotal           = i.UnitPrice * i.Quantity
        }).ToList()
    };
}
