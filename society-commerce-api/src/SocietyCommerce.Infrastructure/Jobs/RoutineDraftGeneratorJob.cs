using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;

namespace SocietyCommerce.Infrastructure.Jobs;

public class RoutineDraftGeneratorJob
{
    private readonly AppDbContext _db;
    private readonly Services.INotificationService _notifier;
    private readonly Services.IAuditService _audit;
    private readonly ILogger<RoutineDraftGeneratorJob> _logger;

    public RoutineDraftGeneratorJob(AppDbContext db, Services.INotificationService notifier,
        Services.IAuditService audit, ILogger<RoutineDraftGeneratorJob> logger)
    {
        _db = db;
        _notifier = notifier;
        _audit = audit;
        _logger = logger;
    }

    public async Task Execute()
    {
        var now = DateTime.UtcNow;

        var dueRoutines = await _db.Routines
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .Include(r => r.Shop)
            .Where(r => !r.IsPaused && r.NextRunAt != null && r.NextRunAt <= now)
            .ToListAsync();

        foreach (var routine in dueRoutines)
        {
            var snapshot = routine.Items.Select(ri => new
            {
                productId = ri.ProductId,
                name = ri.Product.Name,
                quantity = ri.Quantity,
                pricePaise = ri.Product.PricePaise,
                available = ri.Product.IsAvailable && ri.Product.DeletedAt == null
            }).ToList();

            var estimatedTotal = snapshot.Where(s => s.available).Sum(s => s.pricePaise * s.quantity);

            var draft = new DraftOrder
            {
                Id = Guid.NewGuid(),
                RoutineId = routine.Id,
                UserId = routine.UserId,
                ShopId = routine.ShopId,
                Status = DraftStatus.Pending,
                ItemsSnapshot = JsonSerializer.Serialize(snapshot),
                EstimatedTotalPaise = estimatedTotal,
                ScheduledFor = now
            };

            _db.DraftOrders.Add(draft);

            // Advance NextRunAt
            routine.NextRunAt = CalculateNextRun(routine);

            await _notifier.Send(routine.UserId, "routine.draft_ready",
                "routine.draft_ready_title", "routine.draft_ready_body",
                new { routine_label = routine.Label },
                new { draft_id = draft.Id });

            // Auto-pause if all items unavailable
            if (snapshot.All(s => !s.available))
            {
                routine.IsPaused = true;
                await _notifier.Send(routine.UserId, "routine.auto_paused",
                    "routine.auto_paused_title", "routine.auto_paused_body",
                    new { routine_label = routine.Label });
                await _audit.Log("system.routine.auto_paused", "routine", routine.Id,
                    new { reason = "all_items_unavailable" });
            }

            _logger.LogInformation("Generated draft for routine {RoutineId}", routine.Id);
        }

        await _db.SaveChangesAsync();
    }

    private static DateTime CalculateNextRun(Routine r)
    {
        var now = DateTime.UtcNow;
        return r.Frequency switch
        {
            RoutineFrequency.Daily => now.Date.AddDays(1).AddHours(6),
            RoutineFrequency.Weekly => now.Date.AddDays(7).AddHours(6),
            RoutineFrequency.Biweekly => now.Date.AddDays(14).AddHours(6),
            RoutineFrequency.Monthly => now.Date.AddMonths(1).AddHours(6),
            _ => now.Date.AddDays(1).AddHours(6)
        };
    }
}
