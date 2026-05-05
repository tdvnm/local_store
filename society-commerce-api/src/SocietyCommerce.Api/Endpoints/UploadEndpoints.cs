using Microsoft.AspNetCore.Mvc;
using SocietyCommerce.Contracts.Responses;
using SocietyCommerce.Infrastructure.Data;

namespace SocietyCommerce.Api.Endpoints;

public static class UploadEndpoints
{
    public static void MapUploadEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/uploads").WithTags("Uploads").RequireAuthorization();

        group.MapPost("/presign", GetPresignedUrl);
    }

    private static async Task<IResult> GetPresignedUrl(
        [FromBody] PresignRequest req, HttpContext http, IConfiguration config)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(req.ContentType))
            return Results.BadRequest(new { error = "Invalid content type. Allowed: jpeg, png, webp" });

        if (req.FileSizeBytes > 5 * 1024 * 1024) // 5MB max
            return Results.BadRequest(new { error = "File too large. Max 5MB" });

        var userId = http.User.FindFirst("sub")!.Value;
        var ext = req.ContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".bin"
        };

        var key = $"{req.Folder}/{userId}/{Guid.NewGuid()}{ext}";
        var r2Bucket = config["R2:BucketUrl"] ?? "https://pub-placeholder.r2.dev";

        // In production, generate actual R2 presigned URL using AWS SDK with R2 credentials
        // For MVP, return direct upload URL pattern
        var presignedUrl = $"{r2Bucket}/{key}?X-Amz-Expires=300";
        var publicUrl = $"{r2Bucket}/{key}";

        return Results.Ok(new PresignResponse(presignedUrl, publicUrl, key));
    }

    public record PresignRequest(string Folder, string ContentType, long FileSizeBytes);
}
