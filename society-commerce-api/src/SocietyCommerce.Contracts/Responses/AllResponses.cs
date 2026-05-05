namespace SocietyCommerce.Contracts.Responses;

// Auth
public record AuthResponse(string AccessToken, UserInfo User);
public record UserInfo(
    Guid Id, string Name, string Phone, string PreferredLanguage,
    string[] Roles, Guid? FlatId, Guid? ShopId
);

// Shop
public record ShopResponse(
    Guid Id, string Name, string Category, string? Description,
    string? LogoUrl, bool IsActive
);

// Product
public record ProductResponse(
    Guid Id, string Name, string Category, int PricePaise,
    string? Description, string? ImageUrl,
    short InventoryType, int? StockQuantity,
    bool IsAvailable, bool IsRegulated
);

public record ProductAlertResponse(
    Guid Id, string Name, string AlertType,
    int? StockQuantity, DateTime? LastOrderedAt
);

// Cart
public record CartResponse(Guid ShopId, string ShopName, List<CartItemResponse> Items, int TotalPaise);
public record CartItemResponse(
    Guid ProductId, string ProductName, int PricePaise,
    short Quantity, short InventoryType, bool IsAvailable
);

// Order
public record OrderResponse(
    Guid Id, string OrderNumber, Guid ShopId, string ShopName,
    short Status, short FulfillmentType,
    string? PickupCode, int SubtotalPaise, int? ConfirmedTotalPaise,
    string? OrderNotes, string? DeliveryNotes,
    DateTime? ConfirmationExpiresAt, DateTime CreatedAt, DateTime? CompletedAt,
    List<OrderItemResponse> Items,
    DeliveryAssignmentResponse? Delivery
);

public record OrderItemResponse(
    Guid Id, Guid ProductId, string ProductName, int UnitPricePaise,
    short RequestedQuantity, short? ConfirmedQuantity,
    short ItemStatus, string? RejectionReason,
    SubstitutionResponse? Substitution
);

public record SubstitutionResponse(
    Guid Id, Guid SubstituteProductId, string SubstituteProductName,
    short SubstituteQuantity, int SubstitutePricePaise,
    short Status, DateTime ExpiresAt
);

public record DeliveryAssignmentResponse(
    Guid AgentId, string AgentName, short Status,
    DateTime? PickedUpAt, DateTime? DeliveredAt
);

// Order list (compact)
public record OrderSummaryResponse(
    Guid Id, string OrderNumber, string ShopName,
    short Status, short FulfillmentType,
    int? ConfirmedTotalPaise, int SubtotalPaise,
    int ItemCount, DateTime CreatedAt
);

// Delivery Agent
public record AgentResponse(Guid Id, string Name, string Phone, bool IsActive, int ActiveDeliveries);

// Delivery agent view
public record MyDeliveryResponse(
    Guid OrderId, string OrderNumber, string FlatNumber, string? Block,
    string? DeliveryNotes, int ConfirmedTotalPaise,
    short Status, List<DeliveryItemResponse> Items
);
public record DeliveryItemResponse(string Name, short Quantity);

// Routine
public record RoutineResponse(
    Guid Id, string Label, Guid ShopId, string ShopName,
    short Frequency, int? DayOfWeek, int? DayOfMonth,
    bool IsPaused, DateTime? NextRunAt,
    List<RoutineItemResponse> Items
);
public record RoutineItemResponse(Guid ProductId, string ProductName, int PricePaise, short Quantity);

// Draft
public record DraftOrderResponse(
    Guid Id, Guid RoutineId, string RoutineLabel, Guid ShopId, string ShopName,
    short Status, string ItemsSnapshot, int EstimatedTotalPaise,
    DateTime ScheduledFor, DateTime CreatedAt
);

// Admin
public record DashboardResponse(
    int TotalUsers, int TotalShops, int ActiveOrders,
    int TodayOrders, long TodayRevenuePaise,
    int PendingSellers, int OpenTickets
);

public record AuditLogResponse(
    Guid Id, string Action, string EntityType, Guid EntityId,
    Guid? ActorId, string? ActorName, string Metadata, DateTime CreatedAt
);

// Pagination
public record PagedResponse<T>(List<T> Items, int TotalCount, int Page, int PageSize);

// Upload
public record PresignResponse(string UploadUrl, string PublicUrl, string Key);
