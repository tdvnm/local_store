using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Contracts.Requests;
using SocietyCommerce.Contracts.Responses;
using SocietyCommerce.Domain.Entities;
using SocietyCommerce.Infrastructure.Data;

namespace SocietyCommerce.Api.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cart").WithTags("Cart").RequireAuthorization("BuyerPolicy");

        group.MapGet("/{shopId:guid}", GetCart);
        group.MapPost("/{shopId:guid}/items", AddItem);
        group.MapPut("/{shopId:guid}/items/{productId:guid}", UpdateItem);
        group.MapDelete("/{shopId:guid}/items/{productId:guid}", RemoveItem);
        group.MapDelete("/{shopId:guid}", ClearCart);
    }

    private static Guid GetUserId(HttpContext http) => Guid.Parse(http.User.FindFirst("sub")!.Value);

    private static async Task<IResult> GetCart(Guid shopId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var cart = await db.Carts
            .Include(c => c.Items).ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ShopId == shopId);

        if (cart == null)
            return Results.Ok(new CartResponse(shopId, "", new List<CartItemResponse>(), 0));

        var shop = await db.Shops.FirstAsync(s => s.Id == shopId);
        var items = cart.Items.Select(ci => new CartItemResponse(
            ci.ProductId, ci.Product.Name, ci.Product.PricePaise,
            ci.Quantity, (short)ci.Product.InventoryType, ci.Product.IsAvailable
        )).ToList();

        var total = items.Sum(i => i.PricePaise * i.Quantity);
        return Results.Ok(new CartResponse(shopId, shop.Name, items, total));
    }

    private static async Task<IResult> AddItem(
        Guid shopId, [FromBody] AddCartItemRequest req, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == req.ProductId && p.ShopId == shopId && p.IsAvailable);
        if (product == null) return Results.BadRequest(new { error = "Product not available" });

        var cart = await db.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.ShopId == shopId);

        if (cart == null)
        {
            cart = new Cart { Id = Guid.NewGuid(), UserId = userId, ShopId = shopId, UpdatedAt = DateTime.UtcNow };
            db.Carts.Add(cart);
            await db.SaveChangesAsync();
        }

        var existing = await db.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == req.ProductId);
        if (existing != null)
        {
            existing.Quantity += req.Quantity;
        }
        else
        {
            db.CartItems.Add(new CartItem
            {
                Id = Guid.NewGuid(), CartId = cart.Id,
                ProductId = req.ProductId, Quantity = req.Quantity
            });
        }
        cart.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Item added" });
    }

    private static async Task<IResult> UpdateItem(
        Guid shopId, Guid productId, [FromBody] UpdateCartItemRequest req, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var cart = await db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ShopId == shopId);
        if (cart == null) return Results.NotFound();

        var item = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
        if (item == null) return Results.NotFound();

        if (req.Quantity <= 0)
        {
            db.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = req.Quantity;
        }
        cart.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Cart updated" });
    }

    private static async Task<IResult> RemoveItem(
        Guid shopId, Guid productId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var cart = await db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ShopId == shopId);
        if (cart == null) return Results.NotFound();

        var item = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
        if (item != null)
        {
            db.CartItems.Remove(item);
            cart.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        return Results.Ok(new { message = "Item removed" });
    }

    private static async Task<IResult> ClearCart(Guid shopId, HttpContext http, AppDbContext db)
    {
        var userId = GetUserId(http);
        var cart = await db.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.ShopId == shopId);
        if (cart != null)
        {
            db.Carts.Remove(cart); // Cascade deletes items
            await db.SaveChangesAsync();
        }
        return Results.Ok(new { message = "Cart cleared" });
    }
}
