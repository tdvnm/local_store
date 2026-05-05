using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Contracts.Requests;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Services;

namespace SocietyCommerce.Api.Endpoints;

public static class PickupEndpoints
{
    public static void MapPickupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").WithTags("Pickup");

        group.MapGet("/shops/{shopId:guid}/pickup-slots", ListSlots).RequireAuthorization("BuyerPolicy");
        group.MapPost("/shops/{shopId:guid}/pickup-slots", CreateSlot).RequireAuthorization("SellerOwnerPolicy");
        group.MapPut("/shops/{shopId:guid}/pickup-slots/{slotId:guid}", UpdateSlot).RequireAuthorization("SellerOwnerPolicy");
        group.MapDelete("/shops/{shopId:guid}/pickup-slots/{slotId:guid}", DeleteSlot).RequireAuthorization("SellerOwnerPolicy");

        group.MapPost("/orders/{orderId:guid}/mark-ready", MarkReady).RequireAuthorization("SellerStaffPolicy");
        group.MapPost("/orders/{orderId:guid}/mark-collected", MarkCollected).RequireAuthorization("SellerStaffPolicy");
    }

    private static async Task<IResult> ListSlots(Guid shopId, AppDbContext db)
    {
        var slots = await db.PickupSlots.Where(s => s.ShopId == shopId && s.IsActive)
            .OrderBy(s => s.StartTime)
            .Select(s => new { s.Id, s.Label, s.StartTime, s.EndTime, s.MaxOrders, s.IsActive })
            .ToListAsync();
        return Results.Ok(slots);
    }

    private static async Task<IResult> CreateSlot(
        Guid shopId, [FromBody] CreatePickupSlotRequest req, HttpContext http, AppDbContext db)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var slot = new PickupSlot
        {
            Id = Guid.NewGuid(), ShopId = shopId,
            Label = req.Label, StartTime = req.StartTime, EndTime = req.EndTime,
            MaxOrders = req.MaxOrders
        };
        db.PickupSlots.Add(slot);
        await db.SaveChangesAsync();
        return Results.Created($"/api/shops/{shopId}/pickup-slots/{slot.Id}",
            new { slot.Id, slot.Label, slot.StartTime, slot.EndTime, slot.MaxOrders });
    }

    private static async Task<IResult> UpdateSlot(
        Guid shopId, Guid slotId, [FromBody] UpdatePickupSlotRequest req, HttpContext http, AppDbContext db)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var slot = await db.PickupSlots.FirstOrDefaultAsync(s => s.Id == slotId && s.ShopId == shopId);
        if (slot == null) return Results.NotFound();

        if (req.Label != null) slot.Label = req.Label;
        if (req.StartTime.HasValue) slot.StartTime = req.StartTime.Value;
        if (req.EndTime.HasValue) slot.EndTime = req.EndTime.Value;
        if (req.MaxOrders.HasValue) slot.MaxOrders = req.MaxOrders.Value;
        if (req.IsActive.HasValue) slot.IsActive = req.IsActive.Value;

        await db.SaveChangesAsync();
        return Results.Ok(new { slot.Id, slot.Label, slot.StartTime, slot.EndTime, slot.MaxOrders, slot.IsActive });
    }

    private static async Task<IResult> DeleteSlot(
        Guid shopId, Guid slotId, HttpContext http, AppDbContext db)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var slot = await db.PickupSlots.FirstOrDefaultAsync(s => s.Id == slotId && s.ShopId == shopId);
        if (slot == null) return Results.NotFound();

        slot.IsActive = false;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> MarkReady(
        Guid orderId, HttpContext http, AppDbContext db, INotificationService notifier)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null) return Results.Forbid();

        var order = await db.Orders.FirstOrDefaultAsync(o =>
            o.Id == orderId && o.ShopId == Guid.Parse(shopClaim)
            && o.FulfillmentType == FulfillmentType.Pickup
            && o.Status == OrderStatus.Preparing);
        if (order == null) return Results.BadRequest(new { error = "Order not ready for pickup marking" });

        order.Status = OrderStatus.ReadyForPickup;
        order.ReadyAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await notifier.Send(order.UserId, "order.ready_for_pickup",
            "order.status.ready_for_pickup", "order.ready_for_pickup_body",
            new { orderNumber = order.OrderNumber, pickupCode = order.PickupCode },
            new { orderId = order.Id });

        return Results.Ok(new { order.Status, order.PickupCode });
    }

    private static async Task<IResult> MarkCollected(
        Guid orderId, [FromBody] MarkCollectedRequest req, HttpContext http,
        AppDbContext db, INotificationService notifier, IAuditService audit)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null) return Results.Forbid();

        var order = await db.Orders.FirstOrDefaultAsync(o =>
            o.Id == orderId && o.ShopId == Guid.Parse(shopClaim)
            && o.FulfillmentType == FulfillmentType.Pickup
            && o.Status == OrderStatus.ReadyForPickup);
        if (order == null) return Results.BadRequest(new { error = "Order not ready for collection" });

        if (!string.Equals(order.PickupCode, req.PickupCode, StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { error = "Invalid pickup code" });

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;

        // Create ledger entry
        db.LedgerEntries.Add(new LedgerEntry
        {
            Id = Guid.NewGuid(), OrderId = order.Id, ShopId = order.ShopId,
            FlatId = order.FlatId, AmountPaise = order.ConfirmedTotalPaise ?? order.SubtotalPaise,
            EntryType = LedgerEntryType.OrderCompleted
        });

        await db.SaveChangesAsync();
        await audit.Log("seller.pickup.collected", "order", order.Id, new { pickupCode = req.PickupCode });

        await notifier.Send(order.UserId, "order.delivered",
            "order.status.completed", "order.collected_body",
            new { orderNumber = order.OrderNumber });

        return Results.Ok(new { message = "Order collected" });
    }
}
