using SocietyCommerce.Domain.Enums;

namespace SocietyCommerce.Domain.Entities;

public class Society
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PinCode { get; set; }
    public short HouseholdCap { get; set; } = 6;
    public string Settings { get; set; } = "{}"; // JSONB
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Flat> Flats { get; set; } = new List<Flat>();
    public ICollection<Shop> Shops { get; set; } = new List<Shop>();
}

public class Flat
{
    public Guid Id { get; set; }
    public Guid SocietyId { get; set; }
    public string FlatNumber { get; set; } = default!;
    public string? Block { get; set; }
    public short? Floor { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Society Society { get; set; } = default!;
    public ICollection<HouseholdMembership> Memberships { get; set; } = new List<HouseholdMembership>();
}

public class User
{
    public Guid Id { get; set; }
    public string Phone { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public bool IsActive { get; set; } = true;
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    public ICollection<HouseholdMembership> Memberships { get; set; } = new List<HouseholdMembership>();
}

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public RoleType RoleType { get; set; }
    public Guid? ScopeId { get; set; } // flat_id or shop_id
    public Guid? GrantedBy { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = default!;
}

public class HouseholdMembership
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FlatId { get; set; }
    public HouseholdRole Role { get; set; }
    public Guid? InvitedBy { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RemovedAt { get; set; }

    public User User { get; set; } = default!;
    public Flat Flat { get; set; } = default!;
}

public class HouseholdInvite
{
    public Guid Id { get; set; }
    public Guid FlatId { get; set; }
    public string Phone { get; set; } = default!;
    public string InviteCode { get; set; } = default!;
    public Guid InvitedBy { get; set; }
    public InviteStatus Status { get; set; } = InviteStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public Flat Flat { get; set; } = default!;
}

public class Shop
{
    public Guid Id { get; set; }
    public Guid SocietyId { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SuspendedAt { get; set; }

    public Society Society { get; set; } = default!;
    public User Owner { get; set; } = default!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<DeliveryAgent> DeliveryAgents { get; set; } = new List<DeliveryAgent>();
}

public class Product
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Category { get; set; } = default!;
    public int PricePaise { get; set; }
    public string? ImageUrl { get; set; }
    public InventoryType InventoryType { get; set; }
    public int? StockQuantity { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsRegulated { get; set; } = false;
    public int LowStockThreshold { get; set; } = 10;
    public DateTime? LastOrderedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public Shop Shop { get; set; } = default!;
}

public class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public short Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public Cart Cart { get; set; } = default!;
    public Product Product { get; set; } = default!;
}

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public Guid UserId { get; set; }
    public Guid FlatId { get; set; }
    public Guid ShopId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public FulfillmentType FulfillmentType { get; set; }
    public string? PickupCode { get; set; }
    public Guid? PickupSlotId { get; set; }
    public int SubtotalPaise { get; set; }
    public int? ConfirmedTotalPaise { get; set; }
    public string? OrderNotes { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? CancellationReason { get; set; }
    public Guid? CancelledBy { get; set; }
    public DateTime? ConfirmationExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
    public Flat Flat { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public DeliveryAssignment? DeliveryAssignment { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int UnitPricePaise { get; set; }
    public short RequestedQuantity { get; set; }
    public short? ConfirmedQuantity { get; set; }
    public ItemStatus ItemStatus { get; set; } = ItemStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public Order Order { get; set; } = default!;
    public Product Product { get; set; } = default!;
    public Substitution? Substitution { get; set; }
}

public class Substitution
{
    public Guid Id { get; set; }
    public Guid OriginalOrderItemId { get; set; }
    public Guid SubstituteProductId { get; set; }
    public string SubstituteProductName { get; set; } = default!;
    public short SubstituteQuantity { get; set; }
    public int SubstitutePricePaise { get; set; }
    public SubstitutionStatus Status { get; set; } = SubstitutionStatus.Proposed;
    public DateTime ProposedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public OrderItem OriginalOrderItem { get; set; } = default!;
    public Product SubstituteProduct { get; set; } = default!;
}

public class DeliveryAgent
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid ShopId { get; set; }
    public string Name { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeactivatedAt { get; set; }

    public Shop Shop { get; set; } = default!;
}

public class DeliveryAssignment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid AgentId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DeliveryAssignmentStatus Status { get; set; } = DeliveryAssignmentStatus.Assigned;

    public Order Order { get; set; } = default!;
    public DeliveryAgent Agent { get; set; } = default!;
}

public class PickupSlot
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public string? Label { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public short MaxOrders { get; set; } = 10;
    public bool IsActive { get; set; } = true;

    public Shop Shop { get; set; } = default!;
}

public class Routine
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    public string Label { get; set; } = default!;
    public RoutineFrequency Frequency { get; set; }
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public bool IsPaused { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
    public ICollection<RoutineItem> Items { get; set; } = new List<RoutineItem>();
}

public class RoutineItem
{
    public Guid Id { get; set; }
    public Guid RoutineId { get; set; }
    public Guid ProductId { get; set; }
    public short Quantity { get; set; }

    public Routine Routine { get; set; } = default!;
    public Product Product { get; set; } = default!;
}

public class DraftOrder
{
    public Guid Id { get; set; }
    public Guid RoutineId { get; set; }
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    public DraftStatus Status { get; set; } = DraftStatus.Pending;
    public string ItemsSnapshot { get; set; } = "[]"; // JSONB
    public int EstimatedTotalPaise { get; set; }
    public DateTime ScheduledFor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PlacedAt { get; set; }

    public Routine Routine { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
    public Order? PlacedOrder { get; set; }
    public Guid? PlacedOrderId { get; set; }
}

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = default!;
    public string TitleKey { get; set; } = default!;
    public string BodyKey { get; set; } = default!;
    public string Params { get; set; } = "{}"; // JSONB
    public string Data { get; set; } = "{}"; // JSONB
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
}

public class Ticket
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public TicketType Type { get; set; }
    public string Description { get; set; } = default!;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public string? Resolution { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }

    public Order Order { get; set; } = default!;
}

public class LedgerEntry
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid FlatId { get; set; }
    public int AmountPaise { get; set; }
    public LedgerEntryType EntryType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = default!;
}

public class MonthlySummary
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public DateOnly Month { get; set; }
    public int TotalOrders { get; set; }
    public long TotalRevenuePaise { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int? AvgConfirmationSeconds { get; set; }
    public int TimeoutCount { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public Shop Shop { get; set; } = default!;
}

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorName { get; set; }
    public RoleType? ActorRole { get; set; }
    public string Action { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string Metadata { get; set; } = "{}"; // JSONB
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Announcement
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public short Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = default!;
}
