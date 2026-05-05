// Re-export shared types for convenience
export type {
  ProductResponse as Product,
  CartResponse as Cart,
  CartItemResponse as CartItem,
  OrderResponse as Order,
  OrderItemResponse as OrderItem,
  OrderSummaryResponse as OrderSummary,
  ShopResponse as Shop,
  RoutineResponse as Routine,
  DraftOrderResponse as DraftOrder,
  NotificationResponse as Notification,
  DemoUser as User,
} from '@society-commerce/api-client';

export type { MonthlySummary, LedgerEntry, SupportTicket } from '@society-commerce/api-client';

// Enum mappings
export const OrderStatusMap: Record<number, string> = {
  0: "created",
  1: "awaiting_confirmation",
  2: "confirmed",
  3: "partially_confirmed",
  4: "preparing",
  5: "ready_for_pickup",
  6: "out_for_delivery",
  7: "completed",
  8: "cancelled",
};

export const ItemStatusMap: Record<number, string> = {
  0: "pending",
  1: "confirmed",
  2: "partially_confirmed",
  3: "rejected",
  4: "auto_rejected",
  5: "substitution_proposed",
  6: "sub_accepted",
  7: "sub_rejected",
};

export const ItemStatusLabel: Record<number, string> = {
  0: "Pending",
  1: "Confirmed",
  2: "Partial",
  3: "Rejected",
  4: "Auto-rejected",
  5: "Substitution Proposed",
  6: "Substitution Accepted",
  7: "Substitution Rejected",
};

export const FulfillmentTypeMap: Record<number, string> = {
  1: "delivery",
  2: "pickup",
};

export const InventoryTypeMap: Record<number, string> = {
  1: "finite",
  2: "abundant",
};
