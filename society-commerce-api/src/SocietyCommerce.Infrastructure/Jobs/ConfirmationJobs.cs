using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;

namespace SocietyCommerce.Infrastructure.Jobs;

public class ConfirmationTimeoutJob
{
    private readonly AppDbContext _db;
    private readonly IHubContext<SocietyCommerce.Infrastructure.Hubs.OrderHub> _hub;
    private readonly Services.INotificationService _notifier;
    private readonly Services.IAuditService _audit;
    private readonly ILogger<ConfirmationTimeoutJob> _logger;

    public ConfirmationTimeoutJob(AppDbContext db, IHubContext<SocietyCommerce.Infrastructure.Hubs.OrderHub> hub,
        Services.INotificationService notifier, Services.IAuditService audit,
        ILogger<ConfirmationTimeoutJob> logger)
    {
        _db = db;
        _hub = hub;
        _notifier = notifier;
        _audit = audit;
        _logger = logger;
    }

    public async Task Execute(Guid orderId)
    {
        var order = await _db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null || order.Status != OrderStatus.AwaitingConfirmation)
        {
            _logger.LogInformation("Order {OrderId} already resolved, skipping timeout", orderId);
            return;
        }

        var pendingItems = order.Items.Where(i => i.ItemStatus == ItemStatus.Pending).ToList();
        foreach (var item in pendingItems)
        {
            item.ItemStatus = ItemStatus.AutoRejected;
            item.ConfirmedQuantity = 0;
            item.ConfirmedAt = DateTime.UtcNow;
        }

        // Recalculate
        order.ConfirmedTotalPaise = order.Items
            .Where(i => i.ItemStatus is ItemStatus.Confirmed or ItemStatus.PartiallyConfirmed)
            .Sum(i => i.UnitPricePaise * (i.ConfirmedQuantity ?? 0));

        var hasConfirmed = order.Items.Any(i =>
            i.ItemStatus is ItemStatus.Confirmed or ItemStatus.PartiallyConfirmed);

        order.Status = hasConfirmed ? OrderStatus.PartiallyConfirmed : OrderStatus.Cancelled;
        order.ConfirmedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Notify buyer
        await _notifier.Send(order.UserId, "order.confirmation_timeout",
            "order.status.confirmation_timeout_title", "order.status.confirmation_timeout_body",
            new { orderNumber = order.OrderNumber },
            new { orderId = order.Id });

        // Notify seller
        await _notifier.SendToShopOwner(order.ShopId, "seller.confirmation_missed",
            "seller.confirmation_missed_title", "seller.confirmation_missed_body",
            new { orderNumber = order.OrderNumber });

        // Push real-time update to buyer
        await _hub.Clients.Group($"user_{order.UserId}").SendAsync("OrderUpdated", new
        {
            order.Id, order.Status, order.ConfirmedTotalPaise
        });

        await _audit.Log("system.confirmation_timer.expired", "order", order.Id,
            new { auto_rejected_items = pendingItems.Select(i => i.Id).ToList() });

        _logger.LogWarning("Order {OrderId} confirmation timed out. {Count} items auto-rejected",
            orderId, pendingItems.Count);
    }
}

public class ConfirmationReminderJob
{
    private readonly AppDbContext _db;
    private readonly Services.INotificationService _notifier;
    private readonly ILogger<ConfirmationReminderJob> _logger;

    public ConfirmationReminderJob(AppDbContext db, Services.INotificationService notifier,
        ILogger<ConfirmationReminderJob> logger)
    {
        _db = db;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task Execute(Guid orderId)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null || order.Status != OrderStatus.AwaitingConfirmation) return;

        await _notifier.SendToShopOwner(order.ShopId, "seller.confirmation_reminder",
            "seller.confirm.reminder_title", "seller.confirm.reminder_body",
            new { orderNumber = order.OrderNumber, minutesLeft = 2 });

        _logger.LogInformation("Sent confirmation reminder for order {OrderId}", orderId);
    }
}

public class SubstitutionTimeoutJob
{
    private readonly AppDbContext _db;
    private readonly Services.INotificationService _notifier;
    private readonly ILogger<SubstitutionTimeoutJob> _logger;

    public SubstitutionTimeoutJob(AppDbContext db, Services.INotificationService notifier,
        ILogger<SubstitutionTimeoutJob> logger)
    {
        _db = db;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task Execute(Guid substitutionId)
    {
        var sub = await _db.Substitutions
            .Include(s => s.OriginalOrderItem).ThenInclude(oi => oi.Order)
            .FirstOrDefaultAsync(s => s.Id == substitutionId);

        if (sub == null || sub.Status != SubstitutionStatus.Proposed) return;

        sub.Status = SubstitutionStatus.AutoRejected;
        sub.RespondedAt = DateTime.UtcNow;
        sub.OriginalOrderItem.ItemStatus = ItemStatus.SubRejected;

        // Recalculate order total
        var order = sub.OriginalOrderItem.Order;
        order.ConfirmedTotalPaise = (await _db.OrderItems
            .Where(oi => oi.OrderId == order.Id && (oi.ItemStatus == ItemStatus.Confirmed || oi.ItemStatus == ItemStatus.PartiallyConfirmed || oi.ItemStatus == ItemStatus.SubAccepted))
            .SumAsync(oi => oi.UnitPricePaise * (oi.ConfirmedQuantity ?? 0)));

        await _db.SaveChangesAsync();

        await _notifier.Send(order.UserId, "order.substitution_expired",
            "order.substitution_expired_title", "order.substitution_expired_body",
            new { orderNumber = order.OrderNumber });
    }
}
