using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;

namespace SocietyCommerce.Infrastructure.Identity;

public class JwtTokenService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public JwtTokenService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    public string GenerateAccessToken(User user, IEnumerable<UserRole> roles, HouseholdMembership? membership)
    {
        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("phone", user.Phone),
            new("name", user.Name)
        };

        foreach (var role in roles.Where(r => r.RevokedAt == null))
        {
            var roleName = role.RoleType switch
            {
                RoleType.FlatOwner => "flat_owner",
                RoleType.HouseholdMember => "household_member",
                RoleType.SellerOwner => "seller_owner",
                RoleType.SellerManager => "seller_manager",
                RoleType.DeliveryAgent => "delivery_agent",
                RoleType.Admin => "admin",
                _ => "unknown"
            };
            claims.Add(new Claim("role", roleName));

            if (role.ScopeId.HasValue)
            {
                if (role.RoleType is RoleType.SellerOwner or RoleType.SellerManager)
                    claims.Add(new Claim("shop_id", role.ScopeId.Value.ToString()));
            }
        }

        if (membership != null)
        {
            claims.Add(new Claim("flat_id", membership.FlatId.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<(string tokenHash, RefreshToken entity)> GenerateRefreshToken(Guid userId)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var tokenBase64 = Convert.ToBase64String(tokenBytes);
        var tokenHash = ComputeHash(tokenBase64);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenDays"] ?? "30")),
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return (tokenBase64, refreshToken);
    }

    public async Task<RefreshToken?> ValidateRefreshToken(string token)
    {
        var hash = ComputeHash(token);
        return await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == hash &&
                rt.RevokedAt == null &&
                rt.ExpiresAt > DateTime.UtcNow);
    }

    public async Task RevokeRefreshToken(string token)
    {
        var hash = ComputeHash(token);
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash);
        if (rt != null)
        {
            rt.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task RevokeAllTokensForUser(Guid userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
