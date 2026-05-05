using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Identity;

namespace SocietyCommerce.Api.Endpoints;

/// <summary>
/// Development-only auth endpoints that bypass OTP verification.
/// These endpoints are NEVER registered in non-Development environments.
/// </summary>
public static class DevAuthEndpoints
{
    public static void MapDevAuthEndpoints(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        var group = app.MapGroup("/api/auth/dev").WithTags("Dev Auth");

        group.MapPost("/login", DevLogin).AllowAnonymous();
        group.MapGet("/users", ListDevUsers).AllowAnonymous();
        group.MapPost("/reseed", Reseed).AllowAnonymous();
    }

    /// <summary>
    /// Login as any existing user by phone or userId. Issues the same JWT and refresh cookie
    /// as the real verify-otp flow. Development only.
    /// </summary>
    private static async Task<IResult> DevLogin(
        [FromBody] DevLoginDto request,
        AppDbContext db,
        JwtTokenService jwtService,
        HttpContext httpContext)
    {
        // Find user by phone or userId
        Domain.Entities.User? user = null;

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            user = await db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);
        }
        else if (request.UserId.HasValue)
        {
            user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId.Value);
        }

        if (user == null)
            return Results.NotFound(new { error = "User not found. Provide a valid phone or userId." });

        if (!user.IsActive)
            return Results.Problem("User account is not active", statusCode: 403);

        // Load roles and membership — same as verify-otp
        var roles = await db.UserRoles
            .Where(r => r.UserId == user.Id && r.RevokedAt == null)
            .ToListAsync();

        var membership = await db.HouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == user.Id && m.RemovedAt == null);

        // Generate tokens — same path as verify-otp
        var accessToken = jwtService.GenerateAccessToken(user, roles, membership);
        var (refreshTokenValue, _) = await jwtService.GenerateRefreshToken(user.Id);

        // Set refresh token cookie — same as verify-otp
        httpContext.Response.Cookies.Append("refresh_token", refreshTokenValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Allow non-HTTPS in dev
            SameSite = SameSiteMode.Lax, // Relaxed for dev cross-port access
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/api/auth"
        });

        // Derive shopId from seller roles (ScopeId is the shop)
        var shopRole = roles.FirstOrDefault(r =>
            r.RoleType is RoleType.SellerOwner or RoleType.SellerManager or RoleType.DeliveryAgent);

        return Results.Ok(new
        {
            accessToken,
            user = new
            {
                user.Id,
                user.Name,
                user.Phone,
                user.PreferredLanguage,
                roles = roles.Select(r => MapRoleName(r.RoleType)),
                flatId = membership?.FlatId,
                shopId = shopRole?.ScopeId
            }
        });
    }

    /// <summary>
    /// List all users available for dev login. Development only.
    /// </summary>
    private static async Task<IResult> ListDevUsers(AppDbContext db)
    {
        var users = await db.Users
            .Where(u => u.IsActive)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Phone,
                Roles = u.Roles
                    .Where(r => r.RevokedAt == null)
                    .Select(r => r.RoleType)
                    .ToList()
            })
            .ToListAsync();

        // Map enum to snake_case role names matching JWT claims
        var result = users.Select(u => new
        {
            u.Id,
            u.Name,
            u.Phone,
            Roles = u.Roles.Select(MapRoleName).ToList()
        });

        return Results.Ok(result);
    }

    /// <summary>
    /// Re-seed test data. Cleans up and reinserts correct users, roles, shop, and products.
    /// Development only.
    /// </summary>
    private static async Task<IResult> Reseed(AppDbContext db)
    {
        // Use raw ADO.NET to avoid EF Core treating { } in JSON as format placeholders
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM ""UserRoles"" WHERE ""UserId"" IN (
                    '7447e5c4-5994-42a7-af18-0b63e836a655'::uuid,
                    'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e'::uuid,
                    'a0000001-0000-4000-8000-000000000001'::uuid
                );

                INSERT INTO ""Users"" (""Id"", ""Phone"", ""Name"", ""PreferredLanguage"", ""IsActive"", ""ApprovalStatus"", ""CreatedAt"")
                VALUES ('7447e5c4-5994-42a7-af18-0b63e836a655', '9560018536', 'Test Buyer', 'en', true, 1, NOW())
                ON CONFLICT (""Id"") DO UPDATE SET ""Name"" = 'Test Buyer', ""IsActive"" = true;

                INSERT INTO ""Users"" (""Id"", ""Phone"", ""Name"", ""PreferredLanguage"", ""IsActive"", ""ApprovalStatus"", ""CreatedAt"")
                VALUES ('b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', '9876543210', 'Rajesh Kumar', 'en', true, 1, NOW())
                ON CONFLICT (""Id"") DO UPDATE SET ""Name"" = 'Rajesh Kumar', ""IsActive"" = true;

                INSERT INTO ""Users"" (""Id"", ""Phone"", ""Name"", ""PreferredLanguage"", ""IsActive"", ""ApprovalStatus"", ""CreatedAt"")
                VALUES ('a0000001-0000-4000-8000-000000000001', '9000000001', 'Admin User', 'en', true, 1, NOW())
                ON CONFLICT (""Id"") DO UPDATE SET ""Phone"" = '9000000001', ""Name"" = 'Admin User', ""IsActive"" = true;

                INSERT INTO ""UserRoles"" (""Id"", ""UserId"", ""RoleType"", ""ScopeId"", ""GrantedBy"", ""GrantedAt"")
                VALUES ('22222222-2222-4222-8222-222222222222', '7447e5c4-5994-42a7-af18-0b63e836a655', 1, NULL, NULL, NOW());

                INSERT INTO ""UserRoles"" (""Id"", ""UserId"", ""RoleType"", ""ScopeId"", ""GrantedBy"", ""GrantedAt"")
                VALUES ('33333333-3333-4333-8333-333333333333', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 3, 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', NULL, NOW());

                INSERT INTO ""UserRoles"" (""Id"", ""UserId"", ""RoleType"", ""ScopeId"", ""GrantedBy"", ""GrantedAt"")
                VALUES ('44444444-4444-4444-8444-444444444444', 'a0000001-0000-4000-8000-000000000001', 6, NULL, NULL, NOW());

                INSERT INTO ""Societies"" (""Id"", ""Name"", ""Address"", ""City"", ""PinCode"", ""HouseholdCap"", ""Settings"", ""CreatedAt"")
                VALUES ('a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'Green Valley Society', 'Sector 45, Gurugram', 'Gurugram', '122003', 4, '{""allowCod"": true, ""maxOrderValue"": 500000}'::jsonb, NOW())
                ON CONFLICT DO NOTHING;

                INSERT INTO ""Flats"" (""Id"", ""SocietyId"", ""FlatNumber"", ""Block"", ""Floor"", ""IsActive"", ""CreatedAt"")
                VALUES ('f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'A-101', 'A', 1, true, NOW())
                ON CONFLICT DO NOTHING;

                INSERT INTO ""HouseholdMemberships"" (""Id"", ""UserId"", ""FlatId"", ""Role"", ""InvitedBy"", ""JoinedAt"")
                VALUES ('11111111-1111-4111-8111-111111111111', '7447e5c4-5994-42a7-af18-0b63e836a655', 'f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c', 0, NULL, NOW())
                ON CONFLICT (""Id"") DO NOTHING;

                INSERT INTO ""Shops"" (""Id"", ""SocietyId"", ""OwnerId"", ""Name"", ""Category"", ""Description"", ""IsActive"", ""ApprovalStatus"", ""ApprovedAt"", ""CreatedAt"")
                VALUES ('c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'Lucky Store', 'Grocery', 'Your neighbourhood grocery and dairy store', true, 1, NOW(), NOW())
                ON CONFLICT DO NOTHING;

                INSERT INTO ""Products"" (""Id"", ""ShopId"", ""Name"", ""Category"", ""PricePaise"", ""InventoryType"", ""StockQuantity"", ""IsAvailable"", ""IsRegulated"", ""LowStockThreshold"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('d0000001-0000-4000-8000-000000000001', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Amul Taaza Milk 500ml', 'Dairy', 2900, 2, NULL, true, false, 0, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000002', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Amul Gold Milk 500ml', 'Dairy', 3400, 2, NULL, true, false, 0, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000003', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Mother Dairy Full Cream 500ml', 'Dairy', 3200, 2, NULL, true, false, 0, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000004', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Amul Salted Butter 100g', 'Dairy', 5600, 1, 15, true, false, 3, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000005', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Amul Masti Dahi 400g', 'Dairy', 3500, 1, 10, true, false, 2, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000006', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Harvest Gold White Bread 350g', 'Bread', 4000, 1, 8, true, false, 2, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000007', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'English Oven Atta Bread 400g', 'Bread', 5000, 1, 6, true, false, 2, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000008', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Pav (4 pack)', 'Bread', 2000, 2, NULL, true, false, 0, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000009', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Eggs (6 pack)', 'Eggs', 4200, 1, 20, true, false, 5, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000010', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Eggs (12 pack)', 'Eggs', 7800, 1, 12, true, false, 3, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000011', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Aashirvaad Atta 5kg', 'Atta & Rice', 27000, 1, 5, true, false, 2, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000012', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'India Gate Basmati Rice 1kg', 'Atta & Rice', 16000, 1, 8, true, false, 2, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000013', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Toor Dal 1kg', 'Dal & Pulses', 14500, 1, 10, true, false, 3, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000014', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Maggi Noodles 4-pack', 'Snacks', 5600, 1, 25, true, false, 5, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000015', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Lays Classic Salted 52g', 'Snacks', 2000, 1, 30, true, false, 5, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000016', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Coca Cola 750ml', 'Beverages', 3800, 1, 15, true, false, 3, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000017', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Tata Tea Gold 250g', 'Beverages', 12000, 1, 10, true, false, 2, NOW(), NOW()),
                ('d0000001-0000-4000-8000-000000000018', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Surf Excel 1kg', 'Household', 12500, 1, 8, true, false, 2, NOW(), NOW())
                ON CONFLICT DO NOTHING;
            ";
            await cmd.ExecuteNonQueryAsync();
        }
        finally
        {
            await conn.CloseAsync();
        }
        return Results.Ok(new { message = "Test data re-seeded successfully" });
    }

    private static string MapRoleName(RoleType roleType) => roleType switch
    {
        RoleType.FlatOwner => "flat_owner",
        RoleType.HouseholdMember => "household_member",
        RoleType.SellerOwner => "seller_owner",
        RoleType.SellerManager => "seller_manager",
        RoleType.DeliveryAgent => "delivery_agent",
        RoleType.Admin => "admin",
        _ => "unknown"
    };
}

public record DevLoginDto(string? Phone = null, Guid? UserId = null);
