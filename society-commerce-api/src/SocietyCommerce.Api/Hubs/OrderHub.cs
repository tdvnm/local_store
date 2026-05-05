using Microsoft.AspNetCore.SignalR;

namespace SocietyCommerce.Api.Hubs;

public class OrderHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            // If seller, also join shop group
            var shopId = Context.User?.FindFirst("shop_id")?.Value;
            if (shopId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"shop_{shopId}");
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
