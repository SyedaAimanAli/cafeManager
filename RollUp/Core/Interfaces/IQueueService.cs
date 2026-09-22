using RollUp.Application.DTOs;

namespace RollUp.Core.Interfaces;

public interface IQueueService
{
    Task<IEnumerable<QueueEntryDto>> GetActiveQueueAsync(int outletId);
    Task<QueueEntryDto> AddToQueueAsync(int outletId, CreateQueueEntryDto dto);
    Task<QueueEntryDto?> SeatNextAsync(int outletId);
    Task<bool> RemoveFromQueueAsync(int entryId);
}
