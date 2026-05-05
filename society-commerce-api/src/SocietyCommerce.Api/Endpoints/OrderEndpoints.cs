using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Hangfire;
using SocietyCommerce.Infrastructure.Hubs;
using SocietyCommerce.Contracts.Requests;
using SocietyCommerce.Contracts.Responses;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Jobs;
using SocietyCommerce.Infrastructure.Services;

namespace SocietyCommerce.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapPost("/", PlaceOrder).RequireAuthorization("BuyerPolicy");
        group.MapGet("/", ListOrders).RequireAuthorization();
        group.MapGet("/{orderId:guid}", GetOrder).RequireAuthorization();
        group.MapPost("/{orderId:guid}/cancel", CancelOrder).RequireAuthorization("BuyerPolicy");
    }

    private static Guid GetUserId(HttpContext http) => Guid.Parse(http.User.FindFirst("sub")!.Value);

    private static async Task<IResult> PlaceOrder(
        [FromBody] PlaceOrderRequest req, HttpContext http,
        AppDbContext db, IDistributedCache cache,
        INotificationService notifier, IHubContext<OrderHub> hub, IConfiguration config)
    {
        var userId = GetUserId(http);
        var flatClaim = http.User.FindFirst("flat_id")?.Value;
        if (flatClaim == null) return Results.BadRequest(new { error = "No flat associated with account" });
        var flatId = Guid.Parse(flatClaim);

        var cart = await db.Carts.Include(c => c.Items).ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ShopId == req.ShopId);
        if (cart == null || !cart.Items.Any())
            return Results.BadRequest(new { error = "Cart is empty" });

        // Validate stock for finite items
        foreach (var ci in cart.Items.Where(i => i.Product.InventoryType == InventoryType.Finite))
        {
            if (ci.Product.StockQuantity < ci.Quantity)
                return Results.BadRequest(new { error = $"Insufficient stock for {ci.Product.Name}" });
        }

        // Generate order number
        var orderCount = await db.Orders.CountAsync() + 1;
        var orderNumber = $"SC-{orderCount:D5}";

        var hasAbundant = cart.Items.Any(ci => ci.Product.InventoryType == InventoryType.Abundant);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            UserId = userId,
            FlatId = flatId,
            ShopId = req.ShopId,
            FulfillmentType = (FulfillmentType)req.FulfillmentType,
            PickupSlotId = req.PickupSlotId,
            OrderNotes = req.OrderNotes,
            DeliveryNotes = req.DeliveryNotes,
            Status = hasAbundant ? OrderStatus.AwaitingConfirmation : OrderStatus.Confirmed,
            ConfirmationExpiresAt = hasAbundant ? DateTime.UtcNow.AddMinutes(
                int.Parse(config["Timeouts:ConfirmationMinutes"] ?? "5")) : null,
            ConfirmedAt = hasAbundant ? null : DateTime.UtcNow
        };

        // Generate pickup code for pickup orders
        if (order.FulfillmentType == FulfillmentType.Pickup)
        {
            order.PickupCode = Guid.NewGuid().ToString("N")[..4].ToUpper();
        }

        // Create order items
        foreach (var ci in cart.Items)
        {
            var isFinite = ci.Product.InventoryType == InventoryType.Finite;
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                UnitPricePaise = ci.Product.PricePaise,
                RequestedQuantity = ci.Quantity,
                ConfirmedQuantity = isFinite ? ci.Quantity : null,
                ItemStatus = isFinite ? ItemStatus.Confirmed : ItemStatus.Pending,
                ConfirmedAt = isFinite ? DateTime.UtcNow : null
            });

            // Deduct finite stock
            if (isFinite)
            {
                ci.Product.StockQuantity -= ci.Quantity;
                ci.Product.LastOrderedAt = DateTime.UtcNow;
            }
        }

        order.SubtotalPaise = order.Items.Sum(i => i.UnitPricePaise * i.RequestedQuantity);
        if (!hasAbundant)
        {
            order.ConfirmedTotalPaise = order.SubtotalPaise;
        }

        db.Orders.Add(order);
        db.Carts.Remove(cart); // Clear cart after order

        await db.SaveChangesAsync();

        // Schedule timer jobs for abundant items
        if (hasAbundant)
        {
            var timeoutMinutes = int.Parse(config["Timeouts:ConfirmationMinutes"] ?? "5");
            var reminderMinutes = timeoutMinutes - 2;

            var timeoutJobId = BackgroundJob.Schedule<ConfirmationTimeoutJob>(
                job => job.Execute(order.Id), TimeSpan.FromMinutes(timeoutMinutes));
            BackgroundJob.Schedule<ConfirmationReminderJob>(
                job => job.Execute(order.Id), TimeSpan.FromMinutes(Math.Max(1, reminderMinutes)));

            // Store timeout job ID for possible cancellation
            await cache.SetStringAsync($"order:{order.Id}:timeout_job", timeoutJobId,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(timeoutMinutes + 1) });
        }

        // Notify seller
        await notifier.SendToShopOwner(req.ShopId, "order.placed",
            "seller.new_order_title", "seller.new_order_body",
            new { orderNumber, itemCount = order.Items.Count },
            new { orderId = order.Id });

        // Push to seller via SignalR
        await hub.Clients.Group($"shop_{req.ShopId}").SendAsync("NewOrder", new { order.Id, order.OrderNumber });

        return Results.Created($"/api/orders/{order.Id}", MapToResponse(order));
    }

    private static async Task<IResult> ListOrders(
        HttpContext http, AppDbContext db,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] short? status = null)
    {
        var userId = GetUserId(http);
        var isAdmin = http.User.HasClaim("role", "admin");
        var shopClaim = http.User.FindFirst("shop_id")?.Value;

        IQueryable<Order> query;
        if (isAdmin)
            query = db.Orders;
        else if (shopClaim != null)
            query = db.Orders.Where(o => o.ShopId == Guid.Parse(shopClaim));
        else
            query = db.Orders.Where(o => o.UserId == userId);

        if (status.HasValue) query = query.Where(o => o.Status == (OrderStatus)status.Value);

        var total = await query.CountAsync();
        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Include(o => o.Shop).Include(o => o.Items)
            .Select(o => new OrderSummaryResponse(
                o.Id, o.OrderNumber, o.Shop.Name, (short)o.Status, (short)o.FulfillmentType,
                o.ConfirmedTotalPaise, o.SubtotalPaise, o.Items.Count, o.CreatedAt))
            .ToListAsync();

        return Results.Ok(new PagedResponse<OrderSummaryResponse>(orders, total, page, pageSize));
    }

    private static async Task<IResult> GetOrder(Guid orderId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Substitution)
            .Include(o => o.Shop)
            .Include(o => o.DeliveryAssignment).ThenInclude(d => d!.Agent)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return Results.NotFound();

        // Auth check: buyer owns order, seller owns shop, or admin
        var isOwner = order.UserId == userId;
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        var isSeller = shopClaim != null && Guid.Parse(shopClaim) == order.ShopId;
        var isAdmin = http.User.HasClaim("role", "admin");
        if (!isOwner && !isSeller && !isAdmin) return Results.Forbid();

        return Results.Ok(MapToResponse(order));
    }

    private static async Task<IResult> CancelOrder(
        Guid orderId, [FromBody] CancelOrderRequest? req, HttpContext http,
        AppDbContext db, INotificationService notifier)
    {
        var userId = GetUserId(http);
        var order = await db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        if (order == null) return Results.NotFound();

        if (order.Status >= OrderStatus.Preparing)
            return Results.BadRequest(new { error = "Cannot cancel order that is already being prepared" });

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = req?.Reason;
        order.CancelledBy = userId;

        // Restore finite stock
        foreach (var item in order.Items.Where(i => i.ItemStatus == ItemStatus.Confirmed))
        {
            var product = await db.Products.FindAsync(item.ProductId);
            if (product?.InventoryType == InventoryType.Finite && product.StockQuantity != null)
                product.StockQuantity += item.RequestedQuantity;
        }

        await db.SaveChangesAsync();
        await notifier.SendToShopOwner(order.ShopId, "order.cancelled",
            "seller.order_cancelled_title", "seller.order_cancelled_body",
            new { orderNumber = order.OrderNumber });

        return Results.Ok(new { message = "Order cancelled" });
    }

    private static OrderResponse MapToResponse(Order o) => new(
        o.Id, o.OrderNumber, o.ShopId, o.Shop?.Name ?? "", (short)o.Status, (short)o.FulfillmentType,
        o.PickupCode, o.SubtotalPaise, o.ConfirmedTotalPaise,
        o.OrderNotes, o.DeliveryNotes, o.ConfirmationExpiresAt, o.CreatedAt, o.CompletedAt,
        o.Items.Select(i => new OrderItemResponse(
            i.Id, i.ProductId, i.ProductName, i.UnitPricePaise,
            i.RequestedQuantity, i.ConfirmedQuantity, (short)i.ItemStatus, i.RejectionReason,
            i.Substitution != null ? new SubstitutionResponse(
                i.Substitution.Id, i.Substitution.SubstituteProductId,
                i.Substitution.SubstituteProductName, i.Substitution.SubstituteQuantity,
                i.Substitution.SubstitutePricePaise, (short)i.Substitution.Status, i.Substitution.ExpiresAt
            ) : null
        )).ToList(),
        o.DeliveryAssignment != null ? new DeliveryAssignmentResponse(
            o.DeliveryAssignment.AgentId, o.DeliveryAssignment.Agent?.Name ?? "",
            (short)o.DeliveryAssignment.Status, o.DeliveryAssignment.PickedUpAt, o.DeliveryAssignment.DeliveredAt
        ) : null
    );
}
