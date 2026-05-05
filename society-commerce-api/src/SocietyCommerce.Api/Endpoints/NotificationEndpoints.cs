using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Infrastructure.Data;

namespace SocietyCommerce.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications").RequireAuthorization();

        group.MapGet("/", ListNotifications);
        group.MapGet("/unread-count", UnreadCount);
        group.MapPatch("/{notificationId:guid}/read", MarkRead);
        group.MapPatch("/read-all", MarkAllRead);
    }

    private static Guid GetUserId(HttpContext http) => Guid.Parse(http.User.FindFirst("sub")!.Value);

    private static async Task<IResult> ListNotifications(
        HttpContext http, AppDbContext db,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId(http);
        var total = await db.Notifications.CountAsync(n => n.UserId == userId);
        var notifications = await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(n => new
            {
                n.Id, n.Type, n.TitleKey, n.BodyKey,
                n.Params, n.Data, n.IsRead, n.CreatedAt
            })
            .ToListAsync();
        return Results.Ok(new { items = notifications, total, page, pageSize });
    }

    private static async Task<IResult> UnreadCount(HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var count = await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        return Results.Ok(new { count });
    }

    private static async Task<IResult> MarkRead(Guid notificationId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (notification == null) return Results.NotFound();

        notification.IsRead = true;
        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Marked as read" });
    }

    private static async Task<IResult> MarkAllRead(HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        await db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        return Results.Ok(new { message = "All marked as read" });
    }
}
