using RollUp.Application.DTOs;
using RollUp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RollUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueueController : ControllerBase
{
    private readonly IQueueService _queue;

    public QueueController(IQueueService queue) => _queue = queue;

    /// <summary>Get the active queue for an outlet.</summary>
    [HttpGet("{outletId:int}")]
    public async Task<IActionResult> GetQueue(int outletId)
    {
        var entries = await _queue.GetActiveQueueAsync(outletId);
        return Ok(entries);
    }

    /// <summary>Add a customer to the queue.</summary>
    [HttpPost("{outletId:int}")]
    public async Task<IActionResult> AddToQueue(int outletId, [FromBody] CreateQueueEntryDto dto)
    {
        var entry = await _queue.AddToQueueAsync(outletId, dto);
        return CreatedAtAction(nameof(GetQueue), new { outletId }, entry);
    }

    /// <summary>Seat the next customer in the queue.</summary>
    [HttpPost("{outletId:int}/seat-next")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> SeatNext(int outletId)
    {
        var entry = await _queue.SeatNextAsync(outletId);
        return entry is null ? NotFound(new { message = "Queue is empty." }) : Ok(entry);
    }

    /// <summary>Remove a queue entry.</summary>
    [HttpDelete("{entryId:int}")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> Remove(int entryId)
    {
        var ok = await _queue.RemoveFromQueueAsync(entryId);
        return ok ? NoContent() : NotFound();
    }
}
