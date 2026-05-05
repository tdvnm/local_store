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

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin").RequireAuthorization("AdminPolicy");

        // Dashboard
        group.MapGet("/dashboard", GetDashboard);

        // Buyer/Seller approvals
        group.MapGet("/pending-sellers", PendingSellers);
        group.MapPost("/approve-seller/{shopId:guid}", ApproveSeller);
        group.MapPost("/reject-seller/{shopId:guid}", RejectSeller);

        // Force actions
        group.MapPost("/orders/{orderId:guid}/force-cancel", ForceCancel);
        group.MapPost("/orders/{orderId:guid}/force-complete", ForceComplete);

        // Tickets
        group.MapPost("/tickets/{ticketId:guid}/resolve", ResolveTicket);

        // Audit log
        group.MapGet("/audit-log", GetAuditLog);

        // Reports
        group.MapGet("/reports/revenue", RevenueReport);
        group.MapGet("/reports/export", ExportOrders);

        // Announcements
        group.MapGet("/announcements", ListAnnouncements);
        group.MapPost("/announcements", CreateAnnouncement);
        group.MapDelete("/announcements/{announcementId:guid}", DeleteAnnouncement);

        // Society management
        group.MapGet("/societies", ListSocieties);
        group.MapPost("/societies", CreateSociety);
    }

    private static Guid GetUserId(HttpContext http) => Guid.Parse(http.User.FindFirst("sub")!.Value);

    private static async Task<IResult> GetDashboard(AppDbContext db)
    {
        var today = DateTime.UtcNow.Date;
        var dashboard = new DashboardResponse(
            TotalUsers: await db.Users.CountAsync(),
            TotalShops: await db.Shops.CountAsync(),
            ActiveOrders: await db.Orders.CountAsync(o => o.Status < OrderStatus.Completed),
            TodayOrders: await db.Orders.CountAsync(o => o.CreatedAt >= today),
            TodayRevenuePaise: await db.LedgerEntries.Where(e => e.CreatedAt >= today).SumAsync(e => (long)e.AmountPaise),
            PendingSellers: await db.Shops.CountAsync(s => s.ApprovalStatus == ApprovalStatus.Pending),
            OpenTickets: await db.Tickets.CountAsync(t => t.Status == TicketStatus.Open)
        );
        return Results.Ok(dashboard);
    }

    private static async Task<IResult> PendingSellers(AppDbContext db)
    {
        var pending = await db.Shops.Where(s => s.ApprovalStatus == ApprovalStatus.Pending)
            .Include(s => s.Owner)
            .Select(s => new { s.Id, s.Name, s.Category, OwnerName = s.Owner.Name, OwnerPhone = s.Owner.Phone, s.CreatedAt })
            .ToListAsync();
        return Results.Ok(pending);
    }

    private static async Task<IResult> ApproveSeller(
        Guid shopId, AppDbContext db, IAuditService audit, INotificationService notifier)
    {
        var shop = await db.Shops.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Id == shopId);
        if (shop == null) return Results.NotFound();

        shop.ApprovalStatus = ApprovalStatus.Approved;
        shop.IsActive = true;
        shop.ApprovedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.Log("admin.seller.approved", "shop", shop.Id, new { shopName = shop.Name });
        await notifier.Send(shop.OwnerId, "admin.seller_approved",
            "admin.seller_approved_title", "admin.seller_approved_body",
            new { shopName = shop.Name });

        return Results.Ok(new { message = "Shop approved" });
    }

    private static async Task<IResult> RejectSeller(
        Guid shopId, [FromBody] RejectSellerRequest req, AppDbContext db, IAuditService audit, INotificationService notifier)
    {
        var shop = await db.Shops.FirstOrDefaultAsync(s => s.Id == shopId);
        if (shop == null) return Results.NotFound();

        shop.ApprovalStatus = ApprovalStatus.Rejected;
        shop.RejectionReason = req.Reason;
        await db.SaveChangesAsync();

        await audit.Log("admin.seller.rejected", "shop", shop.Id, new { reason = req.Reason });
        await notifier.Send(shop.OwnerId, "admin.seller_rejected",
            "admin.seller_rejected_title", "admin.seller_rejected_body",
            new { shopName = shop.Name, reason = req.Reason });

        return Results.Ok(new { message = "Shop rejected" });
    }

    private static async Task<IResult> ForceCancel(
        Guid orderId, [FromBody] ForceCancelRequest req, HttpContext http,
        AppDbContext db, IAuditService audit, INotificationService notifier, IHubContext<OrderHub> hub)
    {
        var adminId = GetUserId(http);
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return Results.NotFound();

        if (order.Status >= OrderStatus.Completed)
            return Results.BadRequest(new { error = "Cannot cancel completed order" });

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = $"[Admin] {req.Reason}";
        order.CancelledBy = adminId;

        // Restore stock
        foreach (var item in order.Items.Where(i => i.ItemStatus == ItemStatus.Confirmed))
        {
            var product = await db.Products.FindAsync(item.ProductId);
            if (product?.InventoryType == InventoryType.Finite && product.StockQuantity != null)
                product.StockQuantity += item.RequestedQuantity;
        }

        await db.SaveChangesAsync();
        await audit.Log("admin.order.force_cancelled", "order", orderId, new { reason = req.Reason });

        await notifier.Send(order.UserId, "order.cancelled",
            "order.cancelled_title", "order.force_cancelled_body",
            new { orderNumber = order.OrderNumber, reason = req.Reason });

        await hub.Clients.Group($"user_{order.UserId}").SendAsync("OrderUpdated",
            new { orderId, Status = (short)OrderStatus.Cancelled });
        await hub.Clients.Group($"shop_{order.ShopId}").SendAsync("OrderUpdated",
            new { orderId, Status = (short)OrderStatus.Cancelled });

        return Results.Ok(new { message = "Order force-cancelled" });
    }

    private static async Task<IResult> ForceComplete(
        Guid orderId, [FromBody] ForceCompleteRequest req, HttpContext http,
        AppDbContext db, IAuditService audit, INotificationService notifier)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return Results.NotFound();

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;

        db.LedgerEntries.Add(new LedgerEntry
        {
            Id = Guid.NewGuid(), OrderId = order.Id, ShopId = order.ShopId,
            FlatId = order.FlatId, AmountPaise = order.ConfirmedTotalPaise ?? order.SubtotalPaise,
            EntryType = LedgerEntryType.OrderCompleted
        });

        await db.SaveChangesAsync();
        await audit.Log("admin.order.force_completed", "order", orderId, new { reason = req.Reason });

        await notifier.Send(order.UserId, "order.delivered",
            "order.status.completed", "order.force_completed_body",
            new { orderNumber = order.OrderNumber });

        return Results.Ok(new { message = "Order force-completed" });
    }

    private static async Task<IResult> ResolveTicket(
        Guid ticketId, [FromBody] ResolveTicketRequest req, HttpContext http,
        AppDbContext db, IAuditService audit, INotificationService notifier)
    {
        var adminId = GetUserId(http);
        var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null) return Results.NotFound();

        ticket.Status = (TicketStatus)req.Action;
        ticket.Resolution = req.Resolution;
        ticket.AdminNotes = req.AdminNotes;
        ticket.ResolvedAt = DateTime.UtcNow;
        ticket.ResolvedBy = adminId;
        await db.SaveChangesAsync();

        await audit.Log("admin.ticket.resolved", "ticket", ticketId, new { action = req.Action, resolution = req.Resolution });
        await notifier.Send(ticket.UserId, "ticket.resolved",
            "ticket.resolved_title", "ticket.resolved_body",
            new { resolution = req.Resolution });

        return Results.Ok(new { message = "Ticket resolved" });
    }

    private static async Task<IResult> GetAuditLog(
        AppDbContext db, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string? action = null, [FromQuery] Guid? actorId = null)
    {
        var query = db.AuditLogs.AsQueryable();
        if (!string.IsNullOrEmpty(action)) query = query.Where(a => a.Action.Contains(action));
        if (actorId.HasValue) query = query.Where(a => a.ActorId == actorId.Value);

        var total = await query.CountAsync();
        var logs = await query.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AuditLogResponse(a.Id, a.Action, a.EntityType, a.EntityId, a.ActorId, a.ActorName, a.Metadata, a.CreatedAt))
            .ToListAsync();

        return Results.Ok(new PagedResponse<AuditLogResponse>(logs, total, page, pageSize));
    }

    private static async Task<IResult> RevenueReport(
        AppDbContext db, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? shopId)
    {
        var startDate = from ?? DateTime.UtcNow.Date.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;

        var query = db.LedgerEntries.Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate);
        if (shopId.HasValue) query = query.Where(e => e.ShopId == shopId.Value);

        var daily = await query.GroupBy(e => e.CreatedAt.Date)
            .Select(g => new { Date = g.Key, RevenuePaise = g.Sum(e => (long)e.AmountPaise), OrderCount = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var totalRevenue = daily.Sum(d => d.RevenuePaise);
        var totalOrders = daily.Sum(d => d.OrderCount);

        return Results.Ok(new { startDate, endDate, totalRevenue, totalOrders, daily });
    }

    private static async Task<IResult> ExportOrders(
        AppDbContext db, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? shopId)
    {
        var startDate = from ?? DateTime.UtcNow.Date.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;

        var query = db.Orders.Include(o => o.Items).Include(o => o.Shop)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
        if (shopId.HasValue) query = query.Where(o => o.ShopId == shopId.Value);

        var orders = await query.OrderByDescending(o => o.CreatedAt).Take(1000)
            .Select(o => new
            {
                o.OrderNumber, ShopName = o.Shop.Name, Status = o.Status.ToString(),
                o.SubtotalPaise, o.ConfirmedTotalPaise, ItemCount = o.Items.Count,
                FulfillmentType = o.FulfillmentType.ToString(), o.CreatedAt, o.CompletedAt
            })
            .ToListAsync();

        return Results.Ok(orders);
    }

    private static async Task<IResult> ListAnnouncements(AppDbContext db)
    {
        var announcements = await db.Announcements
            .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new { a.Id, a.Title, a.Body, a.Priority, a.CreatedAt, a.ExpiresAt })
            .ToListAsync();
        return Results.Ok(announcements);
    }

    private static async Task<IResult> CreateAnnouncement(
        [FromBody] CreateAnnouncementRequest req, HttpContext http, AppDbContext db, IAuditService audit)
    {
        var announcement = new Announcement
        {
            Id = Guid.NewGuid(), Title = req.Title, Body = req.Body,
            Priority = req.Priority, ExpiresAt = req.ExpiresAt
        };
        db.Announcements.Add(announcement);
        await db.SaveChangesAsync();
        await audit.Log("admin.announcement.created", "announcement", announcement.Id, new { req.Title });
        return Results.Created($"/api/admin/announcements/{announcement.Id}", new { announcement.Id });
    }

    private static async Task<IResult> DeleteAnnouncement(Guid announcementId, AppDbContext db, IAuditService audit)
    {
        var announcement = await db.Announcements.FindAsync(announcementId);
        if (announcement == null) return Results.NotFound();

        announcement.IsActive = false;
        await db.SaveChangesAsync();
        await audit.Log("admin.announcement.deleted", "announcement", announcementId);
        return Results.NoContent();
    }

    private static async Task<IResult> ListSocieties(AppDbContext db)
    {
        var societies = await db.Societies.Include(s => s.Flats)
            .Select(s => new { s.Id, s.Name, s.Address, s.City, s.PinCode, FlatCount = s.Flats.Count, s.CreatedAt })
            .ToListAsync();
        return Results.Ok(societies);
    }

    private static async Task<IResult> CreateSociety(
        [FromBody] CreateSocietyRequest req, AppDbContext db, IAuditService audit)
    {
        var society = new Society
        {
            Id = Guid.NewGuid(), Name = req.Name, Address = req.Address,
            City = req.City, PinCode = req.PinCode
        };
        db.Societies.Add(society);

        // Create flats
        if (req.Flats != null)
        {
            foreach (var flat in req.Flats)
            {
                db.Flats.Add(new Flat
                {
                    Id = Guid.NewGuid(), SocietyId = society.Id,
                    FlatNumber = flat.FlatNumber, Block = flat.Block, Floor = flat.Floor
                });
            }
        }

        await db.SaveChangesAsync();
        await audit.Log("admin.society.created", "society", society.Id, new { req.Name, flatCount = req.Flats?.Count ?? 0 });
        return Results.Created($"/api/admin/societies/{society.Id}", new { society.Id });
    }
}
