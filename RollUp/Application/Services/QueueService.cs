using RollUp.Application.DTOs;
using RollUp.Core.Entities;
using RollUp.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using RollUp.API.Hubs;

namespace RollUp.Application.Services;

public class QueueService : IQueueService
{
    private readonly IRepository<QueueEntry> _queue;
    private readonly IHubContext<QueueHub> _hubContext;

    public QueueService(IRepository<QueueEntry> queue, IHubContext<QueueHub> hubContext)
    {
        _queue      = queue;
        _hubContext = hubContext;
    }

    public async Task<IEnumerable<QueueEntryDto>> GetActiveQueueAsync(int outletId)
    {
        var entries = await _queue.FindAsync(q => q.OutletId == outletId && !q.IsSeated);
        return entries.OrderBy(q => q.Position).Select(MapToDto);
    }

    public async Task<QueueEntryDto> AddToQueueAsync(int outletId, CreateQueueEntryDto dto)
    {
        var maxPos = (await _queue.FindAsync(q => q.OutletId == outletId && !q.IsSeated))
                         .Select(q => q.Position)
                         .DefaultIfEmpty(0)
                         .Max();

        var entry = new QueueEntry
        {
            OutletId     = outletId,
            CustomerName = dto.CustomerName,
            PartySize    = dto.PartySize,
            PhoneNumber  = dto.PhoneNumber,
            Position     = maxPos + 1
        };

        await _queue.AddAsync(entry);
        await _queue.SaveChangesAsync();

        // Broadcast update to queue clients
        await _hubContext.Clients.Group($"queue-{outletId}")
            .SendAsync("QueueUpdated", outletId);

        return MapToDto(entry);
    }

    public async Task<QueueEntryDto?> SeatNextAsync(int outletId)
    {
        var next = await _queue.FirstOrDefaultAsync(
            q => q.OutletId == outletId && !q.IsSeated);
        if (next is null) return null;

        next.IsSeated = true;
        next.SeatedAt = DateTime.UtcNow;
        _queue.Update(next);
        await _queue.SaveChangesAsync();

        await _hubContext.Clients.Group($"queue-{outletId}")
            .SendAsync("QueueUpdated", outletId);

        return MapToDto(next);
    }

    public async Task<bool> RemoveFromQueueAsync(int entryId)
    {
        var entry = await _queue.GetByIdAsync(entryId);
        if (entry is null) return false;

        entry.IsDeleted = true;
        _queue.Update(entry);
        await _queue.SaveChangesAsync();

        await _hubContext.Clients.Group($"queue-{entry.OutletId}")
            .SendAsync("QueueUpdated", entry.OutletId);

        return true;
    }

    private static QueueEntryDto MapToDto(QueueEntry q) => new()
    {
        Id           = q.Id,
        CustomerName = q.CustomerName,
        PartySize    = q.PartySize,
        PhoneNumber  = q.PhoneNumber,
        IsSeated     = q.IsSeated,
        CreatedAt    = q.CreatedAt,
        Position     = q.Position
    };
}
