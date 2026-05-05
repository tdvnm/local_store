using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Contracts.Requests;
using SocietyCommerce.Contracts.Responses;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Services;

namespace SocietyCommerce.Api.Endpoints;

public static class ShopEndpoints
{
    public static void MapShopEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/shops").WithTags("Shops");

        group.MapGet("/", ListShops).RequireAuthorization("BuyerPolicy");
        group.MapGet("/{shopId:guid}", GetShop).RequireAuthorization("BuyerPolicy");
        group.MapPut("/{shopId:guid}", UpdateShop).RequireAuthorization("SellerOwnerPolicy");
        group.MapGet("/{shopId:guid}/stats", GetShopStats).RequireAuthorization("SellerOwnerPolicy");
    }

    private static async Task<IResult> ListShops(AppDbContext db, ICatalogCacheService cache)
    {
        var cacheKey = cache.ShopListKey();
        var cached = await cache.GetAsync<List<ShopResponse>>(cacheKey);
        if (cached is not null) return Results.Ok(cached);

        var shops = await db.Shops.Where(s => s.IsActive)
            .Select(s => new ShopResponse(s.Id, s.Name, s.Category, s.Description, s.LogoUrl, s.IsActive))
            .ToListAsync();

        await cache.SetAsync(cacheKey, shops, TimeSpan.FromMinutes(10));
        return Results.Ok(shops);
    }

    private static async Task<IResult> GetShop(Guid shopId, AppDbContext db, ICatalogCacheService cache)
    {
        var cacheKey = cache.ShopDetailKey(shopId);
        var cached = await cache.GetAsync<ShopResponse>(cacheKey);
        if (cached is not null) return Results.Ok(cached);

        var shop = await db.Shops.FirstOrDefaultAsync(s => s.Id == shopId && s.IsActive);
        if (shop == null) return Results.NotFound();

        var response = new ShopResponse(shop.Id, shop.Name, shop.Category, shop.Description, shop.LogoUrl, shop.IsActive);
        await cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateShop(
        Guid shopId, [FromBody] UpdateShopRequest req, HttpContext http, AppDbContext db, ICatalogCacheService cache)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var shop = await db.Shops.FindAsync(shopId);
        if (shop == null) return Results.NotFound();

        if (req.Name != null) shop.Name = req.Name;
        if (req.Description != null) shop.Description = req.Description;
        if (req.LogoUrl != null) shop.LogoUrl = req.LogoUrl;

        await db.SaveChangesAsync();

        // Invalidate shop caches
        await cache.InvalidateAsync(cache.ShopListKey());
        await cache.InvalidateAsync(cache.ShopDetailKey(shopId));

        return Results.Ok(new ShopResponse(shop.Id, shop.Name, shop.Category, shop.Description, shop.LogoUrl, shop.IsActive));
    }

    private static async Task<IResult> GetShopStats(
        Guid shopId, HttpContext http, AppDbContext db)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var today = DateTime.UtcNow.Date;
        var todayOrders = await db.Orders.CountAsync(o => o.ShopId == shopId && o.CreatedAt >= today);
        var todayRevenue = await db.LedgerEntries
            .Where(e => e.ShopId == shopId && e.CreatedAt >= today)
            .SumAsync(e => (long)e.AmountPaise);

        return Results.Ok(new { todayOrders, todayRevenuePaise = todayRevenue });
    }
}
