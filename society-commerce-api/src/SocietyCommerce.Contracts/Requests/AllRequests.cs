namespace SocietyCommerce.Contracts.Requests;

// Household
public record InviteMemberRequest(string Phone);
public record AcceptInviteRequest(string Name);

// Shops
public record UpdateShopRequest(string? Name, string? Description, string? LogoUrl);

// Products
public record CreateProductRequest(
    string Name,
    string Category,
    int PricePaise,
    string? Description,
    string? ImageUrl,
    short InventoryType,
    int? StockQuantity,
    bool IsRegulated = false,
    int LowStockThreshold = 10
);

public record UpdateProductRequest(
    string? Name,
    string? Category,
    int? PricePaise,
    string? Description,
    string? ImageUrl,
    short? InventoryType,
    int? StockQuantity,
    bool? IsRegulated,
    int? LowStockThreshold
);

public record ToggleAvailabilityRequest(bool IsAvailable);

// Cart
public record AddCartItemRequest(Guid ProductId, short Quantity);
public record UpdateCartItemRequest(short Quantity);

// Orders
public record PlaceOrderRequest(
    Guid ShopId,
    short FulfillmentType,
    Guid? PickupSlotId,
    string? OrderNotes,
    string? DeliveryNotes
);

public record CancelOrderRequest(string? Reason);

// Confirmation
public record ConfirmItemsRequest(List<ItemConfirmation> Items);
public record ItemConfirmation(Guid ItemId, short ConfirmedQuantity, string? Reason);

public record ProposeSubstitutionRequest(
    Guid SubstituteProductId,
    short SubstituteQuantity
);

public record SubstitutionResponseRequest(bool Accept);

// Delivery
public record CreateAgentRequest(string Name, string Phone);
public record UpdateAgentRequest(string? Name, string? Phone);
public record AssignDeliveryRequest(Guid AgentId);

// Pickup slots
public record CreatePickupSlotRequest(string Label, TimeOnly StartTime, TimeOnly EndTime, short MaxOrders = 10);
public record UpdatePickupSlotRequest(string? Label, TimeOnly? StartTime, TimeOnly? EndTime, short? MaxOrders, bool? IsActive);
public record MarkCollectedRequest(string PickupCode);

// Routines
public record CreateRoutineRequest(
    Guid ShopId,
    string Label,
    short Frequency,
    int? DayOfWeek,
    int? DayOfMonth,
    List<RoutineItemInput> Items
);

public record UpdateRoutineRequest(
    string? Label,
    short? Frequency,
    int? DayOfWeek,
    int? DayOfMonth,
    List<RoutineItemInput>? Items
);

public record RoutineItemInput(Guid ProductId, short Quantity);

// Tickets
public record CreateTicketRequest(
    Guid OrderId,
    short Type,
    string Description
);

public record ResolveTicketRequest(short Action, string Resolution, string? AdminNotes);

// Admin
public record RejectSellerRequest(string Reason);
public record ForceCancelRequest(string Reason);
public record ForceCompleteRequest(string Reason);
public record CreateAnnouncementRequest(string Title, string Body, short Priority = 0, DateTime? ExpiresAt = null);
public record CreateSocietyRequest(string Name, string? Address, string? City, string? PinCode, List<CreateFlatInput>? Flats);
public record CreateFlatInput(string FlatNumber, string? Block, short? Floor);
