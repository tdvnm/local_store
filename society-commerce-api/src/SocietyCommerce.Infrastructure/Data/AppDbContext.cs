using Microsoft.EntityFrameworkCore;
using SocietyCommerce.Domain.Entities;

namespace SocietyCommerce.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Society> Societies => Set<Society>();
    public DbSet<Flat> Flats => Set<Flat>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<HouseholdMembership> HouseholdMemberships => Set<HouseholdMembership>();
    public DbSet<HouseholdInvite> HouseholdInvites => Set<HouseholdInvite>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Substitution> Substitutions => Set<Substitution>();
    public DbSet<DeliveryAgent> DeliveryAgents => Set<DeliveryAgent>();
    public DbSet<DeliveryAssignment> DeliveryAssignments => Set<DeliveryAssignment>();
    public DbSet<PickupSlot> PickupSlots => Set<PickupSlot>();
    public DbSet<Routine> Routines => Set<Routine>();
    public DbSet<RoutineItem> RoutineItems => Set<RoutineItem>();
    public DbSet<DraftOrder> DraftOrders => Set<DraftOrder>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<MonthlySummary> MonthlySummaries => Set<MonthlySummary>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filters (soft delete)
        modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
        modelBuilder.Entity<Product>().HasQueryFilter(p => p.DeletedAt == null);

        // Society
        modelBuilder.Entity<Society>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Settings).HasColumnType("jsonb");
        });

        // Flat
        modelBuilder.Entity<Flat>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.SocietyId, x.FlatNumber }).IsUnique();
            e.HasOne(x => x.Society).WithMany(s => s.Flats).HasForeignKey(x => x.SocietyId);
        });

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Phone).IsUnique();
        });

        // UserRole
        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User).WithMany(u => u.Roles).HasForeignKey(x => x.UserId);
        });

        // HouseholdMembership
        modelBuilder.Entity<HouseholdMembership>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.FlatId }).IsUnique();
            e.HasIndex(x => x.FlatId);
            e.HasOne(x => x.User).WithMany(u => u.Memberships).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Flat).WithMany(f => f.Memberships).HasForeignKey(x => x.FlatId);
        });

        // HouseholdInvite
        modelBuilder.Entity<HouseholdInvite>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.InviteCode).IsUnique();
        });

        // Shop
        modelBuilder.Entity<Shop>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Society).WithMany(s => s.Shops).HasForeignKey(x => x.SocietyId);
            e.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId);
        });

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ShopId, x.IsAvailable, x.Category });
            e.HasOne(x => x.Shop).WithMany(s => s.Products).HasForeignKey(x => x.ShopId);
        });

        // Cart
        modelBuilder.Entity<Cart>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.ShopId }).IsUnique();
        });

        // CartItem
        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
            e.HasOne(x => x.Cart).WithMany(c => c.Items).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.HasIndex(x => new { x.UserId, x.Status, x.CreatedAt });
            e.HasIndex(x => new { x.ShopId, x.Status, x.CreatedAt });
            e.HasIndex(x => new { x.FlatId, x.CreatedAt });
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Flat).WithMany().HasForeignKey(x => x.FlatId);
            e.HasOne(x => x.Shop).WithMany().HasForeignKey(x => x.ShopId);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderId);
            e.HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        // Substitution
        modelBuilder.Entity<Substitution>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.OriginalOrderItem).WithOne(oi => oi.Substitution)
                .HasForeignKey<Substitution>(x => x.OriginalOrderItemId);
            e.HasOne(x => x.SubstituteProduct).WithMany().HasForeignKey(x => x.SubstituteProductId);
        });

        // DeliveryAgent
        modelBuilder.Entity<DeliveryAgent>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Shop).WithMany(s => s.DeliveryAgents).HasForeignKey(x => x.ShopId);
        });

        // DeliveryAssignment
        modelBuilder.Entity<DeliveryAssignment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderId);
            e.HasOne(x => x.Order).WithOne(o => o.DeliveryAssignment).HasForeignKey<DeliveryAssignment>(x => x.OrderId);
            e.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId);
        });

        // PickupSlot
        modelBuilder.Entity<PickupSlot>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Shop).WithMany().HasForeignKey(x => x.ShopId);
        });

        // Routine
        modelBuilder.Entity<Routine>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Shop).WithMany().HasForeignKey(x => x.ShopId);
        });

        // RoutineItem
        modelBuilder.Entity<RoutineItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Routine).WithMany(r => r.Items).HasForeignKey(x => x.RoutineId).OnDelete(DeleteBehavior.Cascade);
        });

        // DraftOrder
        modelBuilder.Entity<DraftOrder>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.RoutineId, x.ScheduledFor }).IsUnique();
            e.HasIndex(x => new { x.UserId, x.Status });
            e.Property(x => x.ItemsSnapshot).HasColumnType("jsonb");
            e.HasOne(x => x.Routine).WithMany().HasForeignKey(x => x.RoutineId);
            e.HasOne(x => x.Shop).WithMany().HasForeignKey(x => x.ShopId);
            e.HasOne(x => x.PlacedOrder).WithMany().HasForeignKey(x => x.PlacedOrderId);
        });

        // Notification
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
            e.Property(x => x.Params).HasColumnType("jsonb");
            e.Property(x => x.Data).HasColumnType("jsonb");
        });

        // Ticket
        modelBuilder.Entity<Ticket>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.Status, x.CreatedAt });
            e.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId);
        });

        // LedgerEntry
        modelBuilder.Entity<LedgerEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ShopId, x.CreatedAt });
        });

        // MonthlySummary
        modelBuilder.Entity<MonthlySummary>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ShopId, x.Month }).IsUnique();
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.Action, x.CreatedAt });
            e.HasIndex(x => new { x.EntityType, x.EntityId });
            e.Property(x => x.Metadata).HasColumnType("jsonb");
        });

        // Announcement
        modelBuilder.Entity<Announcement>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });
    }
}
