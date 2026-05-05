using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;

namespace SocietyCommerce.Infrastructure.Services;

public interface IAuditService
{
    Task Log(string action, string entityType, Guid entityId, object? metadata = null,
        Guid? actorUserId = null, RoleType? actorRole = null, string? ipAddress = null);
}

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuditService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task Log(string action, string entityType, Guid entityId, object? metadata = null,
        Guid? actorId = null, RoleType? actorRole = null, string? ipAddress = null)
    {
        var userId = actorId;
        if (userId == null)
        {
            var sub = _http.HttpContext?.User.FindFirst("sub")?.Value;
            if (sub != null) userId = Guid.Parse(sub);
        }

        var role = actorRole;
        if (role == null)
        {
            var roleClaim = _http.HttpContext?.User.FindFirst("role")?.Value;
            if (roleClaim != null && Enum.TryParse<RoleType>(roleClaim, true, out var parsed))
                role = parsed;
        }

        var actorName = _http.HttpContext?.User.FindFirst("name")?.Value;
        var ip = ipAddress ?? _http.HttpContext?.Connection.RemoteIpAddress?.ToString();

        _db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorId = userId,
            ActorName = actorName,
            ActorRole = role,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : "{}",
            IpAddress = ip,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
