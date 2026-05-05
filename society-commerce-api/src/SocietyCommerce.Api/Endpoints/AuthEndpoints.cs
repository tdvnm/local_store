using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Identity;

namespace SocietyCommerce.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/request-otp", RequestOtp)
            .AllowAnonymous()
            .RequireRateLimiting("auth");

        group.MapPost("/verify-otp", VerifyOtp)
            .AllowAnonymous()
            .RequireRateLimiting("auth");

        group.MapPost("/refresh", RefreshToken)
            .AllowAnonymous();

        group.MapPost("/logout", Logout)
            .RequireAuthorization();

        group.MapGet("/me", GetMe)
            .RequireAuthorization();
    }

    private static async Task<IResult> RequestOtp(
        [FromBody] RequestOtpDto request,
        IDistributedCache cache,
        IConfiguration config,
        AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Length < 10)
            return Results.BadRequest(new { error = "Invalid phone number" });

        // Rate limit check via Redis
        var rateLimitKey = $"otp_rate:{request.Phone}";
        var attempts = await cache.GetStringAsync(rateLimitKey);
        if (int.TryParse(attempts, out var count) && count >= 3)
            return Results.Problem("Too many OTP requests. Try again in 15 minutes.", statusCode: 429);

        // Generate OTP (6 digits)
        var otp = Random.Shared.Next(100000, 999999).ToString();

        // Store OTP in Redis with 5-min expiry
        var otpKey = $"otp:{request.Phone}";
        await cache.SetStringAsync(otpKey, otp, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        // Increment rate limit counter
        var newCount = (count + 1).ToString();
        await cache.SetStringAsync(rateLimitKey, newCount, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        });

        // In production, send OTP via SMS provider
        // For dev, log it
        var bypassCode = config["Otp:BypassCode"];
        if (!string.IsNullOrEmpty(bypassCode))
        {
            // Dev mode: always accept bypass code
            await cache.SetStringAsync(otpKey, bypassCode, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }

        return Results.Ok(new { message = "OTP sent", expiresInSeconds = 300 });
    }

    private static async Task<IResult> VerifyOtp(
        [FromBody] VerifyOtpDto request,
        IDistributedCache cache,
        AppDbContext db,
        JwtTokenService jwtService,
        HttpContext httpContext)
    {
        if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Otp))
            return Results.BadRequest(new { error = "Phone and OTP are required" });

        // Verify OTP
        var otpKey = $"otp:{request.Phone}";
        var storedOtp = await cache.GetStringAsync(otpKey);

        if (storedOtp == null || storedOtp != request.Otp)
            return Results.Unauthorized();

        // Clear OTP (one-time use)
        await cache.RemoveAsync(otpKey);

        // Find or identify user
        var user = await db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);

        if (user == null)
        {
            // New user — return a token indicating they need to complete registration
            return Results.Ok(new { requiresRegistration = true, phone = request.Phone });
        }

        if (user.ApprovalStatus == ApprovalStatus.Pending)
            return Results.Ok(new { pendingApproval = true, message = "Your account is pending admin approval" });

        if (user.ApprovalStatus == ApprovalStatus.Rejected || !user.IsActive)
            return Results.Problem("Account is not active", statusCode: 403);

        // Load roles and membership
        var roles = await db.UserRoles
            .Where(r => r.UserId == user.Id && r.RevokedAt == null)
            .ToListAsync();

        var membership = await db.HouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == user.Id && m.RemovedAt == null);

        // Generate tokens
        var accessToken = jwtService.GenerateAccessToken(user, roles, membership);
        var (refreshTokenValue, _) = await jwtService.GenerateRefreshToken(user.Id);

        // Set refresh token as httpOnly cookie
        httpContext.Response.Cookies.Append("refresh_token", refreshTokenValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/api/auth"
        });

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
                roles = roles.Select(r => r.RoleType.ToString().ToLower()),
                flatId = membership?.FlatId,
                shopId = shopRole?.ScopeId
            }
        });
    }

    private static async Task<IResult> RefreshToken(
        HttpContext httpContext,
        JwtTokenService jwtService,
        AppDbContext db)
    {
        var refreshTokenValue = httpContext.Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshTokenValue))
            return Results.Unauthorized();

        var refreshToken = await jwtService.ValidateRefreshToken(refreshTokenValue);
        if (refreshToken == null)
            return Results.Unauthorized();

        var user = refreshToken.User;
        if (!user.IsActive)
            return Results.Unauthorized();

        // Rotate: revoke old, issue new
        await jwtService.RevokeRefreshToken(refreshTokenValue);

        var roles = await db.UserRoles
            .Where(r => r.UserId == user.Id && r.RevokedAt == null)
            .ToListAsync();

        var membership = await db.HouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == user.Id && m.RemovedAt == null);

        var accessToken = jwtService.GenerateAccessToken(user, roles, membership);
        var (newRefreshTokenValue, _) = await jwtService.GenerateRefreshToken(user.Id);

        httpContext.Response.Cookies.Append("refresh_token", newRefreshTokenValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/api/auth"
        });

        return Results.Ok(new { accessToken });
    }

    private static async Task<IResult> Logout(
        HttpContext httpContext,
        JwtTokenService jwtService)
    {
        var refreshTokenValue = httpContext.Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            await jwtService.RevokeRefreshToken(refreshTokenValue);
        }

        httpContext.Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            Path = "/api/auth"
        });

        return Results.Ok(new { message = "Logged out" });
    }

    private static async Task<IResult> GetMe(
        HttpContext httpContext,
        AppDbContext db)
    {
        var userId = httpContext.User.FindFirst("sub")?.Value;
        if (userId == null) return Results.Unauthorized();

        var user = await db.Users
            .Include(u => u.Roles.Where(r => r.RevokedAt == null))
            .Include(u => u.Memberships.Where(m => m.RemovedAt == null))
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

        if (user == null) return Results.NotFound();

        var membership = user.Memberships.FirstOrDefault();
        Flat? flat = null;
        if (membership != null)
        {
            flat = await db.Flats.FirstOrDefaultAsync(f => f.Id == membership.FlatId);
        }

        return Results.Ok(new
        {
            user.Id,
            user.Name,
            user.Phone,
            user.Email,
            user.PreferredLanguage,
            user.AvatarUrl,
            roles = user.Roles.Select(r => new { r.RoleType, r.ScopeId }),
            flat = flat != null ? new { flat.Id, flat.FlatNumber, flat.Block, flat.Floor } : null,
            householdRole = membership?.Role
        });
    }
}

// DTOs
public record RequestOtpDto(string Phone);
public record VerifyOtpDto(string Phone, string Otp);
