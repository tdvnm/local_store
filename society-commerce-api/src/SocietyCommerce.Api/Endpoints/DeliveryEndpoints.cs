using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Infrastructure.Hubs;
using SocietyCommerce.Contracts.Requests;
using SocietyCommerce.Contracts.Responses;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Services;

namespace SocietyCommerce.Api.Endpoints;

public static class DeliveryEndpoints
{
    public static void MapDeliveryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").WithTags("Delivery");

        group.MapGet("/shops/{shopId:guid}/agents", ListAgents).RequireAuthorization("SellerOwnerPolicy");
        group.MapPost("/shops/{shopId:guid}/agents", CreateAgent).RequireAuthorization("SellerOwnerPolicy");
        group.MapPut("/shops/{shopId:guid}/agents/{agentId:guid}", UpdateAgent).RequireAuthorization("SellerOwnerPolicy");
        group.MapDelete("/shops/{shopId:guid}/agents/{agentId:guid}", DeactivateAgent).RequireAuthorization("SellerOwnerPolicy");

        group.MapPost("/orders/{orderId:guid}/start-preparing", StartPreparing).RequireAuthorization("SellerStaffPolicy");
        group.MapPost("/orders/{orderId:guid}/assign-delivery", AssignDelivery).RequireAuthorization("SellerStaffPolicy");
        group.MapPost("/orders/{orderId:guid}/mark-dispatched", MarkDispatched).RequireAuthorization();
        group.MapPost("/orders/{orderId:guid}/mark-delivered", MarkDelivered).RequireAuthorization();
        group.MapGet("/delivery/my-assignments", MyAssignments).RequireAuthorization("DeliveryAgentPolicy");
    }

    private static async Task<IResult> ListAgents(Guid shopId, HttpContext http, AppDbContext db)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var agents = await db.DeliveryAgents.IgnoreQueryFilters()
            .Where(a => a.ShopId == shopId && a.IsActive)
            .Select(a => new AgentResponse(a.Id, a.Name, a.Phone, a.IsActive,
                db.DeliveryAssignments.Count(d => d.AgentId == a.Id && d.Status < DeliveryAssignmentStatus.Delivered)))
            .ToListAsync();
        return Results.Ok(agents);
    }

    private static async Task<IResult> CreateAgent(
        Guid shopId, [FromBody] CreateAgentRequest req, HttpContext http, AppDbContext db, IAuditService audit)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var agent = new DeliveryAgent
        {
            Id = Guid.NewGuid(), ShopId = shopId, Name = req.Name, Phone = req.Phone
        };
        db.DeliveryAgents.Add(agent);
        await db.SaveChangesAsync();
        await audit.Log("seller.agent.created", "delivery_agent", agent.Id, new { req.Name, req.Phone });
        return Results.Created($"/api/shops/{shopId}/agents/{agent.Id}",
            new AgentResponse(agent.Id, agent.Name, agent.Phone, true, 0));
    }

    private static async Task<IResult> UpdateAgent(
        Guid shopId, Guid agentId, [FromBody] UpdateAgentRequest req, HttpContext http, AppDbContext db)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var agent = await db.DeliveryAgents.FirstOrDefaultAsync(a => a.Id == agentId && a.ShopId == shopId);
        if (agent == null) return Results.NotFound();

        if (req.Name != null) agent.Name = req.Name;
        if (req.Phone != null) agent.Phone = req.Phone;
        await db.SaveChangesAsync();
        return Results.Ok(new AgentResponse(agent.Id, agent.Name, agent.Phone, agent.IsActive, 0));
    }

    private static async Task<IResult> DeactivateAgent(
        Guid shopId, Guid agentId, HttpContext http, AppDbContext db, IAuditService audit)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var agent = await db.DeliveryAgents.FirstOrDefaultAsync(a => a.Id == agentId && a.ShopId == shopId);
        if (agent == null) return Results.NotFound();

        agent.IsActive = false;
        agent.DeactivatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await audit.Log("seller.agent.deactivated", "delivery_agent", agent.Id);
        return Results.NoContent();
    }

    private static async Task<IResult> StartPreparing(
        Guid orderId, HttpContext http, AppDbContext db, INotificationService notifier)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null) return Results.Forbid();

        var order = await db.Orders.FirstOrDefaultAsync(o =>
            o.Id == orderId && o.ShopId == Guid.Parse(shopClaim)
            && (o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.PartiallyConfirmed));
        if (order == null) return Results.BadRequest(new { error = "Order not ready for preparation" });

        order.Status = OrderStatus.Preparing;
        await db.SaveChangesAsync();

        await notifier.Send(order.UserId, "order.preparing",
            "order.status.preparing", "order.preparing_body",
            new { orderNumber = order.OrderNumber }, new { orderId = order.Id });

        return Results.Ok(new { order.Status });
    }

    private static async Task<IResult> AssignDelivery(
        Guid orderId, [FromBody] AssignDeliveryRequest req, HttpContext http,
        AppDbContext db, INotificationService notifier, IHubContext<OrderHub> hub)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null) return Results.Forbid();

        var order = await db.Orders.FirstOrDefaultAsync(o =>
            o.Id == orderId && o.ShopId == Guid.Parse(shopClaim) && o.Status == OrderStatus.Preparing);
        if (order == null) return Results.BadRequest(new { error = "Order not in preparing status" });

        var agent = await db.DeliveryAgents.FirstOrDefaultAsync(a => a.Id == req.AgentId && a.IsActive);
        if (agent == null) return Results.BadRequest(new { error = "Agent not found" });

        db.DeliveryAssignments.Add(new DeliveryAssignment
        {
            Id = Guid.NewGuid(), OrderId = orderId, AgentId = req.AgentId
        });
        await db.SaveChangesAsync();

        if (agent.UserId.HasValue)
        {
            await notifier.Send(agent.UserId.Value, "delivery.assigned",
                "delivery.assigned_title", "delivery.assigned_body",
                new { orderNumber = order.OrderNumber });
        }

        return Results.Ok(new { message = "Agent assigned" });
    }

    private static async Task<IResult> MarkDispatched(
        Guid orderId, HttpContext http, AppDbContext db, INotificationService notifier, IHubContext<OrderHub> hub)
    {
        var userId = Guid.Parse(http.User.FindFirst("sub")!.Value);

        // Agent or seller can mark dispatched
        var assignment = await db.DeliveryAssignments
            .Include(d => d.Agent).Include(d => d.Order)
            .FirstOrDefaultAsync(d => d.OrderId == orderId && d.Status == DeliveryAssignmentStatus.Assigned);

        var order = assignment?.Order ?? await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return Results.NotFound();

        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        var isAgent = assignment?.Agent?.UserId == userId;
        var isSeller = shopClaim != null && Guid.Parse(shopClaim) == order.ShopId;
        if (!isAgent && !isSeller) return Results.Forbid();

        if (assignment != null)
        {
            assignment.Status = DeliveryAssignmentStatus.PickedUp;
            assignment.PickedUpAt = DateTime.UtcNow;
        }

        order.Status = OrderStatus.OutForDelivery;
        await db.SaveChangesAsync();

        await notifier.Send(order.UserId, "order.out_for_delivery",
            "order.status.out_for_delivery", "order.out_for_delivery_body",
            new { orderNumber = order.OrderNumber }, new { orderId });

        await hub.Clients.Group($"user_{order.UserId}").SendAsync("OrderUpdated",
            new { orderId, Status = (short)OrderStatus.OutForDelivery });

        return Results.Ok(new { message = "Marked as dispatched" });
    }

    private static async Task<IResult> MarkDelivered(
        Guid orderId, HttpContext http, AppDbContext db,
        INotificationService notifier, IAuditService audit, IHubContext<OrderHub> hub)
    {
        var userId = Guid.Parse(http.User.FindFirst("sub")!.Value);

        // Agent or seller can mark delivered
        var assignment = await db.DeliveryAssignments.Include(d => d.Order)
            .FirstOrDefaultAsync(d => d.OrderId == orderId);

        var order = assignment?.Order ?? await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return Results.NotFound();

        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        var isAgent = assignment?.Agent?.UserId == userId;
        var isSeller = shopClaim != null && Guid.Parse(shopClaim) == order.ShopId;
        if (!isAgent && !isSeller) return Results.Forbid();

        if (assignment != null)
        {
            assignment.Status = DeliveryAssignmentStatus.Delivered;
            assignment.DeliveredAt = DateTime.UtcNow;
        }

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

        if (isSeller && !isAgent)
            await audit.Log("seller.delivery.marked_complete", "order", order.Id, new { bypassed_agent = true });

        await notifier.Send(order.UserId, "order.delivered",
            "order.status.completed", "order.delivered_body",
            new { orderNumber = order.OrderNumber }, new { orderId });

        await hub.Clients.Group($"user_{order.UserId}").SendAsync("OrderUpdated",
            new { orderId, Status = (short)OrderStatus.Completed });

        return Results.Ok(new { message = "Delivered" });
    }

    private static async Task<IResult> MyAssignments(HttpContext http, AppDbContext db)
    {
        var userId = Guid.Parse(http.User.FindFirst("sub")!.Value);
        var assignments = await db.DeliveryAssignments
            .Include(d => d.Order).ThenInclude(o => o.Items)
            .Include(d => d.Order).ThenInclude(o => o.Flat)
            .Where(d => d.Agent.UserId == userId && d.Status < DeliveryAssignmentStatus.Delivered)
            .Select(d => new MyDeliveryResponse(
                d.OrderId, d.Order.OrderNumber, d.Order.Flat.FlatNumber, d.Order.Flat.Block,
                d.Order.DeliveryNotes, d.Order.ConfirmedTotalPaise ?? d.Order.SubtotalPaise,
                (short)d.Status,
                d.Order.Items.Where(i => i.ItemStatus == ItemStatus.Confirmed || i.ItemStatus == ItemStatus.PartiallyConfirmed)
                    .Select(i => new DeliveryItemResponse(i.ProductName, i.ConfirmedQuantity ?? i.RequestedQuantity)).ToList()
            ))
            .ToListAsync();
        return Results.Ok(assignments);
    }
}
