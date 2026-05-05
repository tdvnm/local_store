using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Contracts.Requests;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Services;

namespace SocietyCommerce.Api.Endpoints;

public static class TicketEndpoints
{
    public static void MapTicketEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tickets").WithTags("Tickets").RequireAuthorization();

        group.MapPost("/", CreateTicket);
        group.MapGet("/", ListTickets);
        group.MapGet("/{ticketId:guid}", GetTicket);
    }

    private static Guid GetUserId(HttpContext http) => Guid.Parse(http.User.FindFirst("sub")!.Value);

    private static async Task<IResult> CreateTicket(
        [FromBody] CreateTicketRequest req, HttpContext http, AppDbContext db, IAuditService audit)
    {
        var userId = GetUserId(http);
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderId = req.OrderId,
            Type = (TicketType)req.Type,
            Description = req.Description
        };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
        await audit.Log("ticket.created", "ticket", ticket.Id, new { type = req.Type });
        return Results.Created($"/api/tickets/{ticket.Id}", new { ticket.Id });
    }

    private static async Task<IResult> ListTickets(
        HttpContext http, AppDbContext db,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId(http);
        var isAdmin = http.User.HasClaim("role", "admin");

        IQueryable<Ticket> query = isAdmin ? db.Tickets : db.Tickets.Where(t => t.UserId == userId);

        var total = await query.CountAsync();
        var tickets = await query.OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => new
            {
                t.Id, t.OrderId, Type = (short)t.Type, Status = (short)t.Status,
                t.Description, t.Resolution, t.CreatedAt, t.ResolvedAt
            })
            .ToListAsync();
        return Results.Ok(new { items = tickets, total, page, pageSize });
    }

    private static async Task<IResult> GetTicket(Guid ticketId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var isAdmin = http.User.HasClaim("role", "admin");

        var ticket = await db.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId && (isAdmin || t.UserId == userId));
        if (ticket == null) return Results.NotFound();

        return Results.Ok(new
        {
            ticket.Id, ticket.OrderId, Type = (short)ticket.Type, Status = (short)ticket.Status,
            ticket.Description, ticket.Resolution, ticket.AdminNotes,
            ticket.CreatedAt, ticket.ResolvedAt, ticket.ResolvedBy
        });
    }
}
