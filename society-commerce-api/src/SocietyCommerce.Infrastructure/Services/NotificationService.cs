using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Infrastructure.Data;

namespace SocietyCommerce.Infrastructure.Services;

public interface INotificationService
{
    Task Send(Guid userId, string type, string titleKey, string bodyKey,
        object? @params = null, object? data = null);
    Task SendToShopOwner(Guid shopId, string type, string titleKey, string bodyKey,
        object? @params = null, object? data = null);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<SocietyCommerce.Infrastructure.Hubs.OrderHub> _hub;

    public NotificationService(AppDbContext db, IHubContext<SocietyCommerce.Infrastructure.Hubs.OrderHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task Send(Guid userId, string type, string titleKey, string bodyKey,
        object? @params = null, object? data = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            TitleKey = titleKey,
            BodyKey = bodyKey,
            Params = @params != null ? JsonSerializer.Serialize(@params) : "{}",
            Data = data != null ? JsonSerializer.Serialize(data) : "{}",
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        // Push via SignalR
        await _hub.Clients.Group($"user_{userId}").SendAsync("Notification", new
        {
            notification.Id,
            notification.Type,
            notification.TitleKey,
            notification.BodyKey,
            notification.Params,
            notification.Data,
            notification.CreatedAt
        });
    }

    public async Task SendToShopOwner(Guid shopId, string type, string titleKey, string bodyKey,
        object? @params = null, object? data = null)
    {
        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.Id == shopId);
        if (shop == null) return;
        await Send(shop.OwnerId, type, titleKey, bodyKey, @params, data);
    }
}
