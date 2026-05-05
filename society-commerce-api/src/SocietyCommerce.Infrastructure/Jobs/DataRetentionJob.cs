using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;

namespace SocietyCommerce.Infrastructure.Jobs;

public class DataRetentionJob
{
    private readonly AppDbContext _db;
    private readonly ILogger<DataRetentionJob> _logger;

    public DataRetentionJob(AppDbContext db, ILogger<DataRetentionJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Execute()
    {
        var now = DateTime.UtcNow;

        var deletedNotifications = await _db.Notifications
            .Where(n => n.CreatedAt < now.AddDays(-90))
            .ExecuteDeleteAsync();

        var deletedInvites = await _db.HouseholdInvites
            .Where(i => i.Status != InviteStatus.Accepted && i.ExpiresAt < now.AddDays(-7))
            .ExecuteDeleteAsync();

        var deletedTokens = await _db.RefreshTokens
            .Where(t => t.ExpiresAt < now || t.RevokedAt < now.AddDays(-7))
            .ExecuteDeleteAsync();

        var deletedCarts = await _db.Carts
            .Where(c => c.UpdatedAt < now.AddDays(-30))
            .ExecuteDeleteAsync();

        _logger.LogInformation(
            "Data retention: deleted {Notifications} notifications, {Invites} invites, {Tokens} tokens, {Carts} carts",
            deletedNotifications, deletedInvites, deletedTokens, deletedCarts);
    }
}
