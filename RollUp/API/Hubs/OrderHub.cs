using Microsoft.AspNetCore.SignalR;

namespace RollUp.API.Hubs;

/// <summary>
/// SignalR hub for real-time order status updates.
/// Clients join the group "orders-{outletId}" to receive live order events.
/// </summary>
public class OrderHub : Hub
{
    public async Task JoinOutletGroup(int outletId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"orders-{outletId}");
    }

    public async Task LeaveOutletGroup(int outletId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"orders-{outletId}");
    }
}
