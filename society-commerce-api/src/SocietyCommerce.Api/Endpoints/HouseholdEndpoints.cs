using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Contracts.Requests;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Services;

namespace SocietyCommerce.Api.Endpoints;

public static class HouseholdEndpoints
{
    public static void MapHouseholdEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/household").WithTags("Household").RequireAuthorization();

        group.MapPost("/register", RegisterBuyer).AllowAnonymous();
        group.MapPost("/invite", InviteMember).RequireAuthorization("BuyerPolicy");
        group.MapPost("/accept-invite/{code}", AcceptInvite).AllowAnonymous();
        group.MapGet("/members", GetMembers).RequireAuthorization("BuyerPolicy");
        group.MapDelete("/members/{userId:guid}", RemoveMember).RequireAuthorization("BuyerPolicy");
    }

    private static async Task<IResult> RegisterBuyer(
        [FromBody] RegisterBuyerRequest req, AppDbContext db)
    {
        if (await db.Users.AnyAsync(u => u.Phone == req.Phone))
            return Results.Conflict(new { error = "Phone already registered" });

        var flat = await db.Flats.FirstOrDefaultAsync(f => f.FlatNumber == req.FlatNumber);
        if (flat == null)
            return Results.BadRequest(new { error = "Flat number not found in this society" });

        var user = new User
        {
            Id = Guid.NewGuid(), Phone = req.Phone, Name = req.Name,
            ApprovalStatus = ApprovalStatus.Pending
        };
        db.Users.Add(user);

        db.UserRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            RoleType = RoleType.FlatOwner, ScopeId = flat.Id
        });

        db.HouseholdMemberships.Add(new HouseholdMembership
        {
            Id = Guid.NewGuid(), UserId = user.Id, FlatId = flat.Id,
            Role = HouseholdRole.Owner
        });

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Registration submitted. Pending admin approval.", userId = user.Id });
    }

    private static async Task<IResult> InviteMember(
        [FromBody] InviteMemberRequest req, HttpContext http, AppDbContext db, IAuditService audit)
    {
        var userId = Guid.Parse(http.User.FindFirst("sub")!.Value);
        var membership = await db.HouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == userId && m.RemovedAt == null && m.Role == HouseholdRole.Owner);
        if (membership == null) return Results.Forbid();

        var flat = await db.Flats.Include(f => f.Memberships.Where(m => m.RemovedAt == null))
            .FirstAsync(f => f.Id == membership.FlatId);

        var society = await db.Societies.FirstAsync(s => s.Id == flat.SocietyId);
        if (flat.Memberships.Count >= society.HouseholdCap)
            return Results.BadRequest(new { error = $"Household is full ({society.HouseholdCap} members max)" });

        if (await db.HouseholdMemberships.AnyAsync(m => m.User.Phone == req.Phone && m.RemovedAt == null))
            return Results.BadRequest(new { error = "This phone is already a member of another household" });

        var code = Guid.NewGuid().ToString("N")[..8].ToUpper();
        db.HouseholdInvites.Add(new HouseholdInvite
        {
            Id = Guid.NewGuid(), FlatId = flat.Id, Phone = req.Phone,
            InviteCode = code, InvitedBy = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await db.SaveChangesAsync();
        await audit.Log("household.member.invited", "flat", flat.Id, new { invitee_phone = req.Phone });
        return Results.Ok(new { inviteCode = code, expiresAt = DateTime.UtcNow.AddDays(7) });
    }

    private static async Task<IResult> AcceptInvite(
        string code, [FromBody] AcceptInviteRequest req, AppDbContext db)
    {
        var invite = await db.HouseholdInvites
            .FirstOrDefaultAsync(i => i.InviteCode == code && i.Status == InviteStatus.Pending);
        if (invite == null || invite.ExpiresAt < DateTime.UtcNow)
            return Results.BadRequest(new { error = "Invalid or expired invite" });

        var existing = await db.Users.FirstOrDefaultAsync(u => u.Phone == invite.Phone);
        User user;
        if (existing != null)
        {
            user = existing;
        }
        else
        {
            user = new User
            {
                Id = Guid.NewGuid(), Phone = invite.Phone, Name = req.Name,
                ApprovalStatus = ApprovalStatus.Approved, IsActive = true
            };
            db.Users.Add(user);
        }

        db.UserRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            RoleType = RoleType.HouseholdMember, ScopeId = invite.FlatId,
            GrantedBy = invite.InvitedBy
        });

        db.HouseholdMemberships.Add(new HouseholdMembership
        {
            Id = Guid.NewGuid(), UserId = user.Id, FlatId = invite.FlatId,
            Role = HouseholdRole.Member, InvitedBy = invite.InvitedBy
        });

        invite.Status = InviteStatus.Accepted;
        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Invite accepted", userId = user.Id });
    }

    private static async Task<IResult> GetMembers(HttpContext http, AppDbContext db)
    {
        var userId = Guid.Parse(http.User.FindFirst("sub")!.Value);
        var membership = await db.HouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == userId && m.RemovedAt == null);
        if (membership == null) return Results.NotFound();

        var members = await db.HouseholdMemberships
            .Include(m => m.User)
            .Where(m => m.FlatId == membership.FlatId && m.RemovedAt == null)
            .Select(m => new { m.User.Id, m.User.Name, m.User.Phone, m.Role, m.JoinedAt })
            .ToListAsync();

        var flat = await db.Flats.FirstAsync(f => f.Id == membership.FlatId);
        var society = await db.Societies.FirstAsync(s => s.Id == flat.SocietyId);

        return Results.Ok(new { flatNumber = flat.FlatNumber, block = flat.Block, cap = society.HouseholdCap, members });
    }

    private static async Task<IResult> RemoveMember(
        Guid userId, HttpContext http, AppDbContext db, IAuditService audit)
    {
        var callerId = Guid.Parse(http.User.FindFirst("sub")!.Value);
        var callerMembership = await db.HouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == callerId && m.RemovedAt == null && m.Role == HouseholdRole.Owner);
        if (callerMembership == null) return Results.Forbid();

        var target = await db.HouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == userId && m.FlatId == callerMembership.FlatId && m.RemovedAt == null);
        if (target == null) return Results.NotFound();
        if (target.Role == HouseholdRole.Owner)
            return Results.BadRequest(new { error = "Cannot remove the flat owner" });

        target.RemovedAt = DateTime.UtcNow;
        var roles = await db.UserRoles.Where(r => r.UserId == userId && r.ScopeId == callerMembership.FlatId && r.RevokedAt == null).ToListAsync();
        foreach (var r in roles) r.RevokedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await audit.Log("household.member.removed", "flat", callerMembership.FlatId, new { removed_user_id = userId });
        return Results.Ok(new { message = "Member removed" });
    }
}
