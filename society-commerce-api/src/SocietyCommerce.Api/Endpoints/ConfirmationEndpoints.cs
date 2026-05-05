using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Hangfire;
using SocietyCommerce.Infrastructure.Hubs;
using SocietyCommerce.Contracts.Requests;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Jobs;
using SocietyCommerce.Infrastructure.Services;

namespace SocietyCommerce.Api.Endpoints;

public static class ConfirmationEndpoints
{
    public static void MapConfirmationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Confirmation");

        group.MapPost("/{orderId:guid}/confirm-items", ConfirmItems).RequireAuthorization("SellerStaffPolicy");
        group.MapPost("/{orderId:guid}/items/{itemId:guid}/substitute", ProposeSubstitution).RequireAuthorization("SellerStaffPolicy");
        group.MapPost("/{orderId:guid}/items/{itemId:guid}/substitution/respond", RespondToSubstitution).RequireAuthorization("BuyerPolicy");
    }

    private static async Task<IResult> ConfirmItems(
        Guid orderId, [FromBody] ConfirmItemsRequest req, HttpContext http,
        AppDbContext db, IDistributedCache cache, INotificationService notifier,
        IHubContext<OrderHub> hub, IAuditService audit)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null) return Results.Forbid();

        var order = await db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.ShopId == Guid.Parse(shopClaim));
        if (order == null) return Results.NotFound();

        if (order.Status != OrderStatus.AwaitingConfirmation)
            return Results.BadRequest(new { error = "Order is not awaiting confirmation" });

        if (order.ConfirmationExpiresAt.HasValue && DateTime.UtcNow > order.ConfirmationExpiresAt.Value)
            return Results.BadRequest(new { error = "Confirmation window has expired" });

        foreach (var conf in req.Items)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == conf.ItemId);
            if (item == null || item.ItemStatus != ItemStatus.Pending) continue;

            item.ConfirmedQuantity = conf.ConfirmedQuantity;
            item.ConfirmedAt = DateTime.UtcNow;
            item.RejectionReason = conf.Reason;

            if (conf.ConfirmedQuantity == 0)
                item.ItemStatus = ItemStatus.Rejected;
            else if (conf.ConfirmedQuantity < item.RequestedQuantity)
                item.ItemStatus = ItemStatus.PartiallyConfirmed;
            else
                item.ItemStatus = ItemStatus.Confirmed;

            await audit.Log($"seller.item.{item.ItemStatus.ToString().ToLower()}", "order_item", item.Id,
                new { confirmed_qty = conf.ConfirmedQuantity, requested_qty = item.RequestedQuantity });
        }

        // Recalculate
        order.ConfirmedTotalPaise = order.Items
            .Where(i => i.ItemStatus is ItemStatus.Confirmed or ItemStatus.PartiallyConfirmed)
            .Sum(i => i.UnitPricePaise * (i.ConfirmedQuantity ?? 0));

        // Check if all abundant items resolved
        var allResolved = !order.Items.Any(i => i.ItemStatus == ItemStatus.Pending);
        if (allResolved)
        {
            var hasConfirmed = order.Items.Any(i => i.ItemStatus is ItemStatus.Confirmed or ItemStatus.PartiallyConfirmed);
            var hasRejected = order.Items.Any(i => i.ItemStatus is ItemStatus.Rejected or ItemStatus.AutoRejected);

            order.Status = (hasConfirmed && hasRejected)
                ? OrderStatus.PartiallyConfirmed
                : hasConfirmed ? OrderStatus.Confirmed : OrderStatus.Cancelled;
            order.ConfirmedAt = DateTime.UtcNow;

            // Cancel timeout job
            var jobId = await cache.GetStringAsync($"order:{order.Id}:timeout_job");
            if (!string.IsNullOrEmpty(jobId)) BackgroundJob.Delete(jobId);
        }

        await db.SaveChangesAsync();

        // Push to buyer
        await notifier.Send(order.UserId, "order.items_confirmed",
            "order.items_confirmed_title", "order.items_confirmed_body",
            new { orderNumber = order.OrderNumber },
            new { orderId = order.Id });

        await hub.Clients.Group($"user_{order.UserId}").SendAsync("OrderUpdated",
            new { order.Id, order.Status, order.ConfirmedTotalPaise });

        return Results.Ok(new { order.Status, order.ConfirmedTotalPaise });
    }

    private static async Task<IResult> ProposeSubstitution(
        Guid orderId, Guid itemId, [FromBody] ProposeSubstitutionRequest req,
        HttpContext http, AppDbContext db, INotificationService notifier, IConfiguration config)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null) return Results.Forbid();
        var shopId = Guid.Parse(shopClaim);

        var order = await db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.ShopId == shopId);
        if (order == null) return Results.NotFound();

        var item = order.Items.FirstOrDefault(i => i.Id == itemId && i.ItemStatus == ItemStatus.Pending);
        if (item == null) return Results.BadRequest(new { error = "Item not available for substitution" });

        var subProduct = await db.Products.FirstOrDefaultAsync(p => p.Id == req.SubstituteProductId && p.ShopId == shopId && p.IsAvailable);
        if (subProduct == null) return Results.BadRequest(new { error = "Substitute product not available" });

        var subTimeout = int.Parse(config["Timeouts:SubstitutionMinutes"] ?? "10");

        var substitution = new Substitution
        {
            Id = Guid.NewGuid(),
            OriginalOrderItemId = itemId,
            SubstituteProductId = subProduct.Id,
            SubstituteProductName = subProduct.Name,
            SubstituteQuantity = req.SubstituteQuantity,
            SubstitutePricePaise = subProduct.PricePaise * req.SubstituteQuantity,
            ExpiresAt = DateTime.UtcNow.AddMinutes(subTimeout)
        };

        item.ItemStatus = ItemStatus.SubstitutionProposed;
        db.Substitutions.Add(substitution);
        await db.SaveChangesAsync();

        BackgroundJob.Schedule<SubstitutionTimeoutJob>(
            job => job.Execute(substitution.Id), TimeSpan.FromMinutes(subTimeout));

        await notifier.Send(order.UserId, "order.substitution_proposed",
            "order.substitution_proposed_title", "order.substitution_proposed_body",
            new { orderNumber = order.OrderNumber, originalProduct = item.ProductName, substituteProduct = subProduct.Name },
            new { orderId = order.Id, itemId });

        return Results.Ok(new { substitutionId = substitution.Id });
    }

    private static async Task<IResult> RespondToSubstitution(
        Guid orderId, Guid itemId, [FromBody] SubstitutionResponseRequest req,
        HttpContext http, AppDbContext db, IHubContext<OrderHub> hub)
    {
        var userId = Guid.Parse(http.User.FindFirst("sub")!.Value);
        var order = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Substitution)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        if (order == null) return Results.NotFound();

        var item = order.Items.FirstOrDefault(i => i.Id == itemId);
        if (item?.Substitution == null || item.Substitution.Status != SubstitutionStatus.Proposed)
            return Results.BadRequest(new { error = "No pending substitution" });

        if (DateTime.UtcNow > item.Substitution.ExpiresAt)
            return Results.BadRequest(new { error = "Substitution offer has expired" });

        if (req.Accept)
        {
            item.Substitution.Status = SubstitutionStatus.Accepted;
            item.ItemStatus = ItemStatus.SubAccepted;
            item.ConfirmedQuantity = item.Substitution.SubstituteQuantity;
        }
        else
        {
            item.Substitution.Status = SubstitutionStatus.Rejected;
            item.ItemStatus = ItemStatus.SubRejected;
            item.ConfirmedQuantity = 0;
        }
        item.Substitution.RespondedAt = DateTime.UtcNow;

        // Recalculate order total
        order.ConfirmedTotalPaise = order.Items
            .Where(i => i.ItemStatus is ItemStatus.Confirmed or ItemStatus.PartiallyConfirmed or ItemStatus.SubAccepted)
            .Sum(i =>
            {
                if (i.ItemStatus == ItemStatus.SubAccepted && i.Substitution != null)
                    return i.Substitution.SubstitutePricePaise;
                return i.UnitPricePaise * (i.ConfirmedQuantity ?? 0);
            });

        await db.SaveChangesAsync();

        await hub.Clients.Group($"shop_{order.ShopId}").SendAsync("OrderUpdated",
            new { order.Id, order.Status, order.ConfirmedTotalPaise });

        return Results.Ok(new { order.ConfirmedTotalPaise });
    }
}
