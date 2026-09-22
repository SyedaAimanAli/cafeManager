using Microsoft.AspNetCore.SignalR;

namespace RollUp.API.Hubs;

/// <summary>
/// SignalR hub for real-time queue updates.
/// Clients join the group "queue-{outletId}" to receive updates.
/// </summary>
public class QueueHub : Hub
{
    public async Task JoinOutletGroup(int outletId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"queue-{outletId}");
    }

    public async Task LeaveOutletGroup(int outletId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"queue-{outletId}");
    }
}
