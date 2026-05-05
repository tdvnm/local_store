using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace SocietyCommerce.Infrastructure.Services;

public interface ICatalogCacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class;
    Task InvalidateAsync(string key);
    Task InvalidateByPrefixAsync(string prefix);

    // Convenience keys
    string ShopListKey() => "catalog:shops";
    string ShopProductsKey(Guid shopId, string? category, string? search) =>
        $"catalog:shop:{shopId}:products:{category ?? "all"}:{search ?? ""}";
    string ShopDetailKey(Guid shopId) => $"catalog:shop:{shopId}";
}

public class CatalogCacheService : ICatalogCacheService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CatalogCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var data = await _cache.GetStringAsync(key);
        if (data is null) return null;
        return JsonSerializer.Deserialize<T>(data, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? DefaultTtl
        });
    }

    public async Task InvalidateAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task InvalidateByPrefixAsync(string prefix)
    {
        // IDistributedCache doesn't support prefix deletion natively.
        // For MVP, we invalidate known keys. For scale, use StackExchange.Redis directly.
        // This is a no-op placeholder — specific invalidation calls will target exact keys.
        await Task.CompletedTask;
    }
}
