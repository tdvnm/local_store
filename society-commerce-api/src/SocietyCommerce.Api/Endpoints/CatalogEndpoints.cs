using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Contracts.Requests;
using SocietyCommerce.Contracts.Responses;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Domain.Enums;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Services;

namespace SocietyCommerce.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").WithTags("Catalog");

        group.MapGet("/shops/{shopId:guid}/products", ListProducts).RequireAuthorization("BuyerPolicy");
        group.MapPost("/shops/{shopId:guid}/products", CreateProduct).RequireAuthorization("SellerStaffPolicy");
        group.MapPut("/shops/{shopId:guid}/products/{productId:guid}", UpdateProduct).RequireAuthorization("SellerStaffPolicy");
        group.MapDelete("/shops/{shopId:guid}/products/{productId:guid}", DeleteProduct).RequireAuthorization("SellerStaffPolicy");
        group.MapPatch("/shops/{shopId:guid}/products/{productId:guid}/availability", ToggleAvailability).RequireAuthorization("SellerStaffPolicy");
        group.MapGet("/shops/{shopId:guid}/products/alerts", GetAlerts).RequireAuthorization("SellerStaffPolicy");
        group.MapGet("/search", SearchProducts).RequireAuthorization("BuyerPolicy");
    }

    private static async Task<IResult> ListProducts(
        Guid shopId, [FromQuery] string? category, [FromQuery] string? q,
        AppDbContext db, ICatalogCacheService cache)
    {
        var cacheKey = cache.ShopProductsKey(shopId, category, q);
        var cached = await cache.GetAsync<List<ProductResponse>>(cacheKey);
        if (cached is not null) return Results.Ok(cached);

        var query = db.Products.Where(p => p.ShopId == shopId && p.IsAvailable);
        if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category == category);
        if (!string.IsNullOrEmpty(q)) query = query.Where(p => EF.Functions.ILike(p.Name, $"%{q}%"));

        var products = await query.OrderBy(p => p.Category).ThenBy(p => p.Name)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Category, p.PricePaise,
                p.Description, p.ImageUrl, (short)p.InventoryType, p.StockQuantity, p.IsAvailable, p.IsRegulated))
            .ToListAsync();

        // Cache for 2 minutes (products change more often than shops)
        await cache.SetAsync(cacheKey, products, TimeSpan.FromMinutes(2));
        return Results.Ok(products);
    }

    private static async Task<IResult> CreateProduct(
        Guid shopId, [FromBody] CreateProductRequest req, HttpContext http,
        AppDbContext db, ICatalogCacheService cache)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var product = new Product
        {
            Id = Guid.NewGuid(), ShopId = shopId, Name = req.Name,
            Category = req.Category, PricePaise = req.PricePaise,
            Description = req.Description, ImageUrl = req.ImageUrl,
            InventoryType = (InventoryType)req.InventoryType,
            StockQuantity = req.InventoryType == 1 ? req.StockQuantity : null,
            IsRegulated = req.IsRegulated, LowStockThreshold = req.LowStockThreshold
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Invalidate product list caches for this shop
        await InvalidateShopProductCaches(shopId, cache);

        return Results.Created($"/api/shops/{shopId}/products/{product.Id}",
            new ProductResponse(product.Id, product.Name, product.Category, product.PricePaise,
                product.Description, product.ImageUrl, (short)product.InventoryType,
                product.StockQuantity, product.IsAvailable, product.IsRegulated));
    }

    private static async Task<IResult> UpdateProduct(
        Guid shopId, Guid productId, [FromBody] UpdateProductRequest req, HttpContext http,
        AppDbContext db, ICatalogCacheService cache)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.ShopId == shopId);
        if (product == null) return Results.NotFound();

        if (req.Name != null) product.Name = req.Name;
        if (req.Category != null) product.Category = req.Category;
        if (req.PricePaise.HasValue) product.PricePaise = req.PricePaise.Value;
        if (req.Description != null) product.Description = req.Description;
        if (req.ImageUrl != null) product.ImageUrl = req.ImageUrl;
        if (req.InventoryType.HasValue) product.InventoryType = (InventoryType)req.InventoryType.Value;
        if (req.StockQuantity.HasValue) product.StockQuantity = req.StockQuantity.Value;
        if (req.IsRegulated.HasValue) product.IsRegulated = req.IsRegulated.Value;
        if (req.LowStockThreshold.HasValue) product.LowStockThreshold = req.LowStockThreshold.Value;
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // Invalidate product list caches for this shop
        await InvalidateShopProductCaches(shopId, cache);

        return Results.Ok(new ProductResponse(product.Id, product.Name, product.Category, product.PricePaise,
            product.Description, product.ImageUrl, (short)product.InventoryType,
            product.StockQuantity, product.IsAvailable, product.IsRegulated));
    }

    private static async Task<IResult> DeleteProduct(
        Guid shopId, Guid productId, HttpContext http, AppDbContext db, ICatalogCacheService cache)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.ShopId == shopId);
        if (product == null) return Results.NotFound();

        product.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Invalidate product list caches for this shop
        await InvalidateShopProductCaches(shopId, cache);

        return Results.NoContent();
    }

    private static async Task<IResult> ToggleAvailability(
        Guid shopId, Guid productId, [FromBody] ToggleAvailabilityRequest req, HttpContext http,
        AppDbContext db, ICatalogCacheService cache)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.ShopId == shopId);
        if (product == null) return Results.NotFound();

        product.IsAvailable = req.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Invalidate product list caches for this shop
        await InvalidateShopProductCaches(shopId, cache);

        return Results.Ok(new { product.Id, product.IsAvailable });
    }

    private static async Task<IResult> GetAlerts(Guid shopId, HttpContext http, AppDbContext db)
    {
        var shopClaim = http.User.FindFirst("shop_id")?.Value;
        if (shopClaim == null || Guid.Parse(shopClaim) != shopId) return Results.Forbid();

        var lowStock = await db.Products.IgnoreQueryFilters()
            .Where(p => p.ShopId == shopId && p.DeletedAt == null
                && p.InventoryType == InventoryType.Finite
                && p.StockQuantity <= p.LowStockThreshold)
            .Select(p => new ProductAlertResponse(p.Id, p.Name, "low_stock", p.StockQuantity, p.LastOrderedAt))
            .ToListAsync();

        var stale = await db.Products
            .Where(p => p.ShopId == shopId && p.IsAvailable
                && (p.LastOrderedAt == null || p.LastOrderedAt < DateTime.UtcNow.AddDays(-30)))
            .Select(p => new ProductAlertResponse(p.Id, p.Name, "stale", p.StockQuantity, p.LastOrderedAt))
            .ToListAsync();

        return Results.Ok(new { lowStock, stale });
    }

    private static async Task<IResult> SearchProducts([FromQuery] string q, [FromQuery] Guid? shopId, AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2) return Results.Ok(Array.Empty<object>());

        var query = db.Products.Where(p => p.IsAvailable && EF.Functions.ILike(p.Name, $"%{q}%"));
        if (shopId.HasValue) query = query.Where(p => p.ShopId == shopId.Value);

        var results = await query.Take(20).Include(p => p.Shop)
            .Select(p => new { p.Id, p.Name, p.PricePaise, p.ImageUrl, ShopId = p.ShopId, ShopName = p.Shop.Name })
            .ToListAsync();
        return Results.Ok(results);
    }

    /// <summary>
    /// Invalidates the default (unfiltered) product list cache for a shop.
    /// Filtered/search caches expire naturally via TTL.
    /// </summary>
    private static async Task InvalidateShopProductCaches(Guid shopId, ICatalogCacheService cache)
    {
        // Always invalidate the "all products, no filter" cache
        await cache.InvalidateAsync(cache.ShopProductsKey(shopId, null, null));
    }
}
