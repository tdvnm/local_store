using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SocietyCommerce.Infrastructure.Data;
using SocietyCommerce.Infrastructure.Services;
using SocietyCommerce.Api.Endpoints;
using SocietyCommerce.Api.Hubs;
using SocietyCommerce.Infrastructure.Hubs;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, config) => config
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "society-commerce-api"));

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "sc_";
});

// Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };

        // Allow SignalR to use access token from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("BuyerPolicy", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("role", "flat_owner") ||
            ctx.User.HasClaim("role", "household_member")))
    .AddPolicy("SellerOwnerPolicy", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("role", "seller_owner")))
    .AddPolicy("SellerStaffPolicy", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("role", "seller_owner") ||
            ctx.User.HasClaim("role", "seller_manager")))
    .AddPolicy("DeliveryAgentPolicy", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("role", "delivery_agent")))
    .AddPolicy("AdminPolicy", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("role", "admin")));

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<SocietyCommerce.Application.Marker>());

// Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Default")!)));
builder.Services.AddHangfireServer(options => options.WorkerCount = 4);

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Society Commerce API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
});

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
    });
    options.AddFixedWindowLimiter("general", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

// Application services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SocietyCommerce.Infrastructure.Identity.JwtTokenService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<ICatalogCacheService, CatalogCacheService>();

var app = builder.Build();

// Middleware pipeline
app.UseSerilogRequestLogging();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();

    // Auto-migrate in development
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Register Hangfire recurring jobs
using (var jobScope = app.Services.CreateScope())
{
    var jobManager = jobScope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    jobManager.AddOrUpdate<SocietyCommerce.Infrastructure.Jobs.RoutineDraftGeneratorJob>(
        "routine-draft-generator", job => job.Execute(), "*/15 * * * *");
    jobManager.AddOrUpdate<SocietyCommerce.Infrastructure.Jobs.DataRetentionJob>(
        "data-retention", job => job.Execute(), "0 3 * * *");
}

// Map endpoints
app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapDevAuthEndpoints(); // Conditionally registers only in Development
app.MapHouseholdEndpoints();
app.MapShopEndpoints();
app.MapCatalogEndpoints();
app.MapCartEndpoints();
app.MapOrderEndpoints();
app.MapConfirmationEndpoints();
app.MapDeliveryEndpoints();
app.MapPickupEndpoints();
app.MapRoutineEndpoints();
app.MapNotificationEndpoints();
app.MapTicketEndpoints();
app.MapAdminEndpoints();
app.MapUploadEndpoints();

// SignalR hubs
app.MapHub<SocietyCommerce.Infrastructure.Hubs.OrderHub>("/hubs/orders");

app.Run();
