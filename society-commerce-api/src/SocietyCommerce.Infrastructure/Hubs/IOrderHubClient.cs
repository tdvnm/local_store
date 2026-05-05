using Microsoft.AspNetCore.SignalR;

namespace SocietyCommerce.Infrastructure.Hubs;

/// <summary>
/// Marker hub for SignalR. Defined in Infrastructure to avoid circular dependency.
/// The Api project's OrderHub should inherit from this.
/// </summary>
public class OrderHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        var shopId = Context.User?.FindFirst("shop_id")?.Value;

        if (userId != null) await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        if (shopId != null) await Groups.AddToGroupAsync(Context.ConnectionId, $"shop_{shopId}");

        await base.OnConnectedAsync();
    }
}
