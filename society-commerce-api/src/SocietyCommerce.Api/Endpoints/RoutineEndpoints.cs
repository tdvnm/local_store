using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Contracts.Requests;
using SocietyCommerce.Contracts.Responses;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;

namespace SocietyCommerce.Api.Endpoints;

public static class RoutineEndpoints
{
    public static void MapRoutineEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/routines").WithTags("Routines").RequireAuthorization("BuyerPolicy");

        group.MapGet("/", ListRoutines);
        group.MapPost("/", CreateRoutine);
        group.MapPut("/{routineId:guid}", UpdateRoutine);
        group.MapDelete("/{routineId:guid}", DeleteRoutine);
        group.MapPatch("/{routineId:guid}/pause", PauseRoutine);
        group.MapPatch("/{routineId:guid}/resume", ResumeRoutine);

        var drafts = app.MapGroup("/api/drafts").WithTags("Routines").RequireAuthorization("BuyerPolicy");
        drafts.MapGet("/", ListDrafts);
        drafts.MapPost("/{draftId:guid}/place", PlaceDraft);
        drafts.MapPost("/{draftId:guid}/skip", SkipDraft);
    }

    private static Guid GetUserId(HttpContext http) => Guid.Parse(http.User.FindFirst("sub")!.Value);

    private static async Task<IResult> ListRoutines(HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var routines = await db.Routines.Include(r => r.Items).ThenInclude(ri => ri.Product)
            .Include(r => r.Shop)
            .Where(r => r.UserId == userId)
            .Select(r => new RoutineResponse(
                r.Id, r.Label, r.ShopId, r.Shop.Name, (short)r.Frequency,
                r.DayOfWeek, r.DayOfMonth, r.IsPaused, r.NextRunAt,
                r.Items.Select(i => new RoutineItemResponse(i.ProductId, i.Product.Name, i.Product.PricePaise, i.Quantity)).ToList()
            ))
            .ToListAsync();
        return Results.Ok(routines);
    }

    private static async Task<IResult> CreateRoutine(
        [FromBody] CreateRoutineRequest req, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var shop = await db.Shops.FirstOrDefaultAsync(s => s.Id == req.ShopId && s.IsActive);
        if (shop == null) return Results.BadRequest(new { error = "Shop not found" });

        var productIds = req.Items.Select(i => i.ProductId).ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id) && p.ShopId == req.ShopId && p.IsAvailable)
            .ToListAsync();
        if (products.Count != req.Items.Count)
            return Results.BadRequest(new { error = "Some products not available" });

        var routine = new Routine
        {
            Id = Guid.NewGuid(), UserId = userId, ShopId = req.ShopId,
            Label = req.Label, Frequency = (RoutineFrequency)req.Frequency,
            DayOfWeek = req.DayOfWeek, DayOfMonth = req.DayOfMonth,
            NextRunAt = CalculateNextRun((RoutineFrequency)req.Frequency, req.DayOfWeek, req.DayOfMonth)
        };

        foreach (var ri in req.Items)
        {
            routine.Items.Add(new RoutineItem
            {
                Id = Guid.NewGuid(), RoutineId = routine.Id,
                ProductId = ri.ProductId, Quantity = ri.Quantity
            });
        }

        db.Routines.Add(routine);
        await db.SaveChangesAsync();
        return Results.Created($"/api/routines/{routine.Id}", new { routine.Id });
    }

    private static async Task<IResult> UpdateRoutine(
        Guid routineId, [FromBody] UpdateRoutineRequest req, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var routine = await db.Routines.Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == routineId && r.UserId == userId);
        if (routine == null) return Results.NotFound();

        if (req.Label != null) routine.Label = req.Label;
        if (req.Frequency.HasValue)
        {
            routine.Frequency = (RoutineFrequency)req.Frequency.Value;
            routine.DayOfWeek = req.DayOfWeek;
            routine.DayOfMonth = req.DayOfMonth;
            routine.NextRunAt = CalculateNextRun(routine.Frequency, routine.DayOfWeek, routine.DayOfMonth);
        }

        if (req.Items != null)
        {
            db.RoutineItems.RemoveRange(routine.Items);
            foreach (var ri in req.Items)
            {
                routine.Items.Add(new RoutineItem
                {
                    Id = Guid.NewGuid(), RoutineId = routine.Id,
                    ProductId = ri.ProductId, Quantity = ri.Quantity
                });
            }
        }

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Routine updated" });
    }

    private static async Task<IResult> DeleteRoutine(Guid routineId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var routine = await db.Routines.FirstOrDefaultAsync(r => r.Id == routineId && r.UserId == userId);
        if (routine == null) return Results.NotFound();

        db.Routines.Remove(routine);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> PauseRoutine(Guid routineId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var routine = await db.Routines.FirstOrDefaultAsync(r => r.Id == routineId && r.UserId == userId);
        if (routine == null) return Results.NotFound();

        routine.IsPaused = true;
        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Routine paused" });
    }

    private static async Task<IResult> ResumeRoutine(Guid routineId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var routine = await db.Routines.FirstOrDefaultAsync(r => r.Id == routineId && r.UserId == userId);
        if (routine == null) return Results.NotFound();

        routine.IsPaused = false;
        routine.NextRunAt = CalculateNextRun(routine.Frequency, routine.DayOfWeek, routine.DayOfMonth);
        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Routine resumed" });
    }

    private static async Task<IResult> ListDrafts(HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var drafts = await db.DraftOrders
            .Include(d => d.Shop).Include(d => d.Routine)
            .Where(d => d.Routine.UserId == userId && d.Status == DraftStatus.Pending)
            .OrderBy(d => d.CreatedAt)
            .Select(d => new DraftOrderResponse(
                d.Id, d.RoutineId, d.Routine.Label, d.ShopId, d.Shop.Name,
                (short)d.Status, d.ItemsSnapshot, d.EstimatedTotalPaise, d.ScheduledFor, d.CreatedAt
            ))
            .ToListAsync();
        return Results.Ok(drafts);
    }

    private static async Task<IResult> PlaceDraft(Guid draftId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var draft = await db.DraftOrders
            .Include(d => d.Routine)
            .FirstOrDefaultAsync(d => d.Id == draftId && d.Routine.UserId == userId && d.Status == DraftStatus.Pending);
        if (draft == null) return Results.NotFound();

        // Place draft as a real order by populating cart then calling order logic
        // For simplicity, mark draft as placed and redirect to order placement
        draft.Status = DraftStatus.Placed;
        draft.PlacedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // The actual order creation follows the same flow as cart → order
        // Client should parse ItemsSnapshot and post to /api/orders
        return Results.Ok(new { message = "Draft approved for placement", draft.ItemsSnapshot, draft.ShopId });
    }

    private static async Task<IResult> SkipDraft(Guid draftId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var draft = await db.DraftOrders
            .Include(d => d.Routine)
            .FirstOrDefaultAsync(d => d.Id == draftId && d.Routine.UserId == userId && d.Status == DraftStatus.Pending);
        if (draft == null) return Results.NotFound();

        draft.Status = DraftStatus.Skipped;
        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Draft skipped" });
    }

    private static DateTime CalculateNextRun(RoutineFrequency freq, int? dayOfWeek, int? dayOfMonth)
    {
        var now = DateTime.UtcNow;
        return freq switch
        {
            RoutineFrequency.Daily => now.Date.AddDays(1).AddHours(6), // 6 AM UTC next day
            RoutineFrequency.Weekly => now.Date.AddDays(
                ((dayOfWeek ?? 1) - (int)now.DayOfWeek + 7) % 7 is var d && d == 0 ? 7 : d
            ).AddHours(6),
            RoutineFrequency.Biweekly => now.Date.AddDays(14).AddHours(6),
            RoutineFrequency.Monthly => new DateTime(
                dayOfMonth > now.Day ? now.Year : now.AddMonths(1).Year,
                dayOfMonth > now.Day ? now.Month : now.AddMonths(1).Month,
                Math.Min(dayOfMonth ?? 1, DateTime.DaysInMonth(now.Year, now.Month)),
                6, 0, 0, DateTimeKind.Utc),
            _ => now.Date.AddDays(1).AddHours(6)
        };
    }
}
