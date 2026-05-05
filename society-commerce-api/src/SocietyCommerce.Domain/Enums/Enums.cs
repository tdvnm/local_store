namespace SocietyCommerce.Domain.Enums;

public enum RoleType : short
{
    FlatOwner = 1,
    HouseholdMember = 2,
    SellerOwner = 3,
    SellerManager = 4,
    DeliveryAgent = 5,
    Admin = 6
}

public enum ApprovalStatus : short
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum InventoryType : short
{
    Finite = 1,
    Abundant = 2
}

public enum OrderStatus : short
{
    Created = 0,
    AwaitingConfirmation = 1,
    Confirmed = 2,
    PartiallyConfirmed = 3,
    Preparing = 4,
    ReadyForPickup = 5,
    OutForDelivery = 6,
    Completed = 7,
    Cancelled = 8
}

public enum ItemStatus : short
{
    Pending = 0,
    Confirmed = 1,
    PartiallyConfirmed = 2,
    Rejected = 3,
    AutoRejected = 4,
    SubstitutionProposed = 5,
    SubAccepted = 6,
    SubRejected = 7
}

public enum FulfillmentType : short
{
    Delivery = 1,
    Pickup = 2
}

public enum SubstitutionStatus : short
{
    Proposed = 0,
    Accepted = 1,
    Rejected = 2,
    AutoRejected = 3
}

public enum HouseholdRole : short
{
    Owner = 1,
    Member = 2
}

public enum InviteStatus : short
{
    Pending = 0,
    Accepted = 1,
    Expired = 2
}

public enum DraftStatus : short
{
    Pending = 0,
    Placed = 1,
    Skipped = 2
}

public enum RoutineFrequency : short
{
    Daily = 1,
    Weekdays = 2,
    Weekly = 3,
    Biweekly = 4,
    Monthly = 5,
    Custom = 6
}

public enum TicketType : short
{
    MissingItem = 1,
    WrongItem = 2,
    QualityIssue = 3,
    DeliveryIssue = 4,
    Other = 5
}

public enum TicketStatus : short
{
    Open = 0,
    Assigned = 1,
    InResolution = 2,
    Resolved = 3,
    Closed = 4
}

public enum DeliveryAssignmentStatus : short
{
    Assigned = 0,
    PickedUp = 1,
    Delivered = 2,
    Reassigned = 3
}

public enum LedgerEntryType : short
{
    OrderCompleted = 1,
    OrderCancelledAfterConfirm = 2
}

public enum TicketAction : short
{
    Explained = 1,
    Redeliver = 2,
    Cancel = 3,
    WarnSeller = 4,
    SuspendSeller = 5
}
